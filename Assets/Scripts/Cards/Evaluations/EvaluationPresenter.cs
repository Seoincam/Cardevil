using Cardevil.Cards.Data;
using Cardevil.Cards.InStage.Model;
using Cardevil.Cards.InStage.Model.ReadOnly;
using Cardevil.Cards.InStage.Presenter;
using Cardevil.Events;
using Cardevil.Events.ExecEvents;
using Cardevil.Utils;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Cardevil.Cards.Evaluations
{
    public interface IEvaluationPresenter
    {
        /// <summary>
        /// 족보 표시 텍스트를 지움.
        /// </summary>
        void ClearHandRankingText();
        
        /// <summary>
        /// 족보 표시 텍스트 갱신.
        /// 선택된 카드 목록을 기반으로 족보 평가 후 표시 텍스트 업데이트.
        /// </summary>
        /// <param name="selection">선택된 카드 목록</param>
        void UpdateHandRankingText(IEnumerable<Card> selection);

        /// <summary>
        /// 사용한 카드를 등록.
        /// 정렬된 카드 목록을 기반으로 족보 등을 미리 계산함.
        /// </summary>
        void RegisterUsingCards(IReadOnlyList<Card> sortedCards);
        
        /// <summary>
        /// 평가 시퀀스 실행.
        /// 단계별 평가 및 뷰 연출 처리.
        /// </summary>
        UniTask ExcuteSequenceAsync();
        
        /// <returns>가장 최근의 평과 결과를 반환.</returns>
        EvaluationResult GetCurrentEvaluationResult();
        
        IReadOnlyEvaluationResultsModel ResultsModel { get; }

        /// <summary>
        /// <see cref="CardDamageEvaluationArgs"/>를 플레이어의 카드 사용 기반으로 초기화하여 반환함.
        /// </summary>
        CardDamageEvaluationArgs GetArgs();
    }
    
    public class EvaluationPresenter : IEvaluationPresenter
    {
        private EvaluationResultsModel _model;
        private EvaluationView _view;
        private EvaluationSequenceFactory _factory;
        
        private EvaluationSequence _seq;
        private EvaluationResult.Builder _resultBuilder;

        public IReadOnlyEvaluationResultsModel ResultsModel => _model;

        public CardDamageEvaluationArgs GetArgs() => CardDamageEvaluationArgs.Get(_usingCards, _handRanking);

        private IReadOnlyList<Card> _usingCards;
        private HandRanking _handRanking;
        
        private int _attackCardsUsingIndex;

        public void Init(EvaluationResultsModel model)
        {
            if (model == null)
            {
                LogEx.LogError("Model is null");
                return;
            }
            _model = model;
            
            _factory = new EvaluationSequenceFactory(model);
            
            // view 생성
            var canvasName = "CardCanvas";
            var canvas = GameObject.Find(canvasName).transform;
            if (!canvas)
            {
                LogEx.LogError($"Canvas not found. : {canvasName}");
                return;
            }
            
            string path = "UI/CardUI/Evaluation View";
            var go = AssetUtil.Instantiate(path, canvas).gameObject;
            if (!go)
            {
                LogEx.LogError($"Evaluation UI Animator가 존재하지 않음! path: {path}");
                return;
            }
            _view = go.GetComponent<EvaluationView>();
            
            int priority = (int)CardDamageEvaluationArgs.Order.RegisterOnModel;
            ExecStaticEventBus<CardDamageEvaluationArgs>.Register(priority, OnEvaluationEnded);
        }
        
        public void ClearHandRankingText()
        {
            _view.UpdateHandRankingText(HandRanking.None);
        }
        
        public void UpdateHandRankingText(IEnumerable<Card> selection)
        {
            var handRanking = HandRankingEvaluator.EvaluateHandRanking(selection);
            _view.UpdateHandRankingText(handRanking);
        }

        public void RegisterUsingCards(IReadOnlyList<Card> sortedCards)
        {
            _usingCards = sortedCards;
            _handRanking = HandRankingEvaluator.EvaluateHandRanking(sortedCards);
            
            // 0. 이미 유물 등은 static으로 등록되어있음.
            // 1. 카드를 하나씩 등록함 (Dynamic)
            // 2. MergeAndInvoke()
            // 3. Args에 족보 + 데미지 정보가 쌓임
            // 마지막에 적한테 가할 수 있게 TurnManager에 넘겨주면 될 듯.
            
            ExecDynamicEventBus<CardDamageEvaluationArgs>.Register(OnCalculateDamage);
        }
        
        private void OnCalculateDamage(ExecQueue<CardDamageEvaluationArgs> queue, CardDamageEvaluationArgs args)
        {
            _attackCardsUsingIndex = 0;
            for (int i = 0; i < _usingCards.Count; i++)
            {
                int priority = (int)CardDamageEvaluationArgs.Order.PlusCardDamage;
                queue.Enqueue(priority, AddCardDamageAsync);
            }
            return;
            
            async UniTask AddCardDamageAsync(CardDamageEvaluationArgs evaluationArgs, CancellationToken cancellationToken)
            {
                int damageAmount = _usingCards[_attackCardsUsingIndex++].Data.FinalNumber;
                await EvaluationView.Current.DoStep(damageAmount, EvaluationView.EvaluationType.Plus);
                // TODO:
                // 카드별 기본 데미지 유물 호출을 위해 새로운 args 정의,
                // 호출해야함.
                
                evaluationArgs.AddDamage(damageAmount);
            }
        }

        private async UniTask OnEvaluationEnded(CardDamageEvaluationArgs args, CancellationToken token)
        {
            var result = args.BuildResult();
            _model.Add(result);
        }

        
        public async UniTask ExcuteSequenceAsync()
        {
            LogEx.LogWarning("서인 - 아직 Excute 로직 미구현.");
            
            // _seq.Build();
            // await _view.Clear();
            //
            // int index = 0;
            // float totalDamage = 0f;
            // while (_seq.TryGetStepGroup(index++, out List<EvaluationStep> stepGroup))
            // {
            //     if (stepGroup == null || stepGroup.Count == 0) 
            //         continue;
            //     
            //     for (int i = 0; i < stepGroup.Count; i++)
            //     {
            //         var step = stepGroup[i];
            //         step.CalculateDamage(ref totalDamage);
            //         step.ExecuteVisualEffect();
            //         _view.RegisterStep(step);
            //         
            //         await UniTask.Delay(TimeSpan.FromSeconds(.3f));
            //     }
            //     
            //     await _view.DoStep(totalDamage);
            //     await UniTask.Delay(TimeSpan.FromSeconds(.2f));
            // }
            // _view.Clear().Forget();
            //
            // var result = _resultBuilder
            //     .SetDamage((int)Math.Round(totalDamage))
            //     .Build();
            // _model.Add(result);
        }

        public EvaluationResult GetCurrentEvaluationResult()
        {
            return _model.CurrentResult;
        }
    }
}