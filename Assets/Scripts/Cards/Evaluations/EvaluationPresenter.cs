using Cardevil.Cards.Data;
using Cardevil.Cards.InStage.Model;
using Cardevil.Cards.InStage.Model.ReadOnly;
using Cardevil.Cards.InStage.Presenter;
using Cardevil.Core.Bootstrap;
using Cardevil.Events;
using Cardevil.Events.ExecEvents;
using Cardevil.InGame.SlotMachine.Cards.Utils;
using Cardevil.Utils;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
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
        
        IReadOnlyEvaluationResultsModel ResultsModel { get; }
        
        /// <returns>가장 최근의 평과 결과를 반환.</returns>
        EvaluationResult? GetCurrentEvaluationResult();
        
        /// <summary>
        /// 모든 카드를 사용함.
        /// 이때 이동 카드는 즉발, 공격 카드는 머리 위에 쌓임.
        /// </summary>
        UniTask UseAllCardsAsync(CancellationToken cancellationToken);

        UniTask<EvaluationResult> EvaluateAsync(CancellationToken cancellationToken);
    }
    
    public class EvaluationPresenter : IEvaluationPresenter
    {
        private EvaluationResultsModel _model;
        private EvaluationView _view;
        private EvaluationSequenceFactory _factory;
        
        private EvaluationSequence _seq;

        public IReadOnlyEvaluationResultsModel ResultsModel => _model;

        public CardDamageEvaluationArgs GetArgs() => CardDamageEvaluationArgs.Get(_toUseCards, _handRanking);

        private Card[] _toUseCards;
        private HandRanking _handRanking;
        private TileVector _playerPosition;
        
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
            
            // TODO: View는 씬에 박아놔도 될 듯.
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

            // 정적 이벤트 등록
            int clearView = (int)CardDamageEvaluationArgs.Order.ClearView;
            ExecStaticEventBus<CardDamageEvaluationArgs>.Register(clearView, ClearView);
            
            int registerOnModel = (int)CardDamageEvaluationArgs.Order.RegisterOnModel;
            ExecStaticEventBus<CardDamageEvaluationArgs>.Register(registerOnModel, OnEvaluationEnded);

            int playerMovingEnd = (int)PlayerMoveArgs.Orders.Last;
            ExecStaticEventBus<PlayerMoveArgs>.Register(playerMovingEnd, OnPlayerMovingEnded);
            
            // TODO: 나중에 모두 해제해야함.
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
        
        /*
         * 0. 이동과 공격이 섞여있을 때:
         * 이동 카드는 먼저 사용되고, 공격 카드는 머리 위로 올라갔다가 한 번에 시전되어야함.
         * 이동 카드도 이벤트 기반을 쓰긴 해야할 듯.
         * 하나씩 순회하면서 사용 혹은 머리 위를 결정해야하는데,
         * 이 역할은 EvaluationPresenter에서 끝내기 -> 이동까지는 TurnManager 안 넘어가고 EvaluationPresenter에서 처리
         * 
         * 그럼 어케 플레이어에게 이동을 요청?
         * -> 이동 Args를 하나더 정의해서?
         * -> PlayerMovingArgs : 딱 하나의 방향만 등록되어 있음.
         * 
         * 얘를 들어 상, 좌 카드를 사용하면 Args(Up), Args(Left)로 두 번 Invoke.
         * -> 일단 하나의 방향이 아니라, Args에 여러 개의 Direction이 등록 가능하게 해야할듯.
         * WHY? 아이템이나, 유물의 효과로 이동을 두 배로 만드는 등 간섭이 있을 수 있음.
         * 
         */
        
        /*
         * 머리 
         */

        // 0. 이미 유물 등은 static으로 등록되어있음.
        // 1. 카드를 하나씩 등록함 (Dynamic)
        // 2. MergeAndInvoke() (turn manager)
        // 3. Args에 족보 + 데미지 정보가 쌓임
        // 마지막에 적한테 가할 수 있게 TurnManager에 넘겨주면 될 듯.
        public void RegisterUsingCards(IReadOnlyList<Card> sortedCards)
        {
            _toUseCards = sortedCards.ToArray();
            _handRanking = HandRankingEvaluator.EvaluateHandRanking(sortedCards);
            
            ExecDynamicEventBus<CardDamageEvaluationArgs>.Register(HandleEvaluation);
        }
        public async UniTask UseAllCardsAsync(CancellationToken cancellationToken)
        {
            var usingCards = _toUseCards.ToArray();

            for (int i = 0; i < usingCards.Length; i++)
            {
                var card = usingCards[i];
                
                if (card.Data.IsMove)
                {
                    var movingArgs = PlayerMoveArgs.Get(card.Data.FinalDirection);
                    await ExecEventBus<PlayerMoveArgs>.InvokeMergedAndDispose(movingArgs, cancellationToken);
                    
                    // TODO: 
                    // Stage cards Presenter -> evaluation presenter로 넘어올 때
                    // 모델에선 따로 보관해두어야할 듯.
                    // Evaluation Presenter에선 단순 제거만.
                    
                    // TODO:
                    // 제거되는 이펙트와 함께 제거되어야함.
                    _toUseCards[i] = null;
                }
                else if (card.Data.IsAttack)
                {
                    // TODO:
                    // 머리 위에 쌓이기
                }
            }
        }

        public async UniTask<EvaluationResult> EvaluateAsync(CancellationToken cancellationToken)
        {
            var args = CardDamageEvaluationArgs.Get(_toUseCards, _handRanking);
            await ExecEventBus<CardDamageEvaluationArgs>.InvokeMergedAndDispose(args, cancellationToken);

            if (_model.CurrentResult == null)
                LogEx.LogError("서인 - 데미지 계산이 끝난 시점에서 Result에 등록이 안 됐음.");
            
            return (EvaluationResult)_model.CurrentResult;
        }
        
        // 이벤트 버스에 동적으로 평가 이벤트를 등록함.
        // 족보 기본 데미지, 카드 기본 데미지 등을 다룸.
        private void HandleEvaluation(ExecQueue<CardDamageEvaluationArgs> queue, CardDamageEvaluationArgs args)
        {
            // 족보 기본 데미지
            if (_handRanking > HandRanking.High)
            {
                int priority = (int)CardDamageEvaluationArgs.Order.HandRankingDamage;
                queue.Enqueue(priority, AddHandRankingDamage);
                
                async UniTask AddHandRankingDamage(CardDamageEvaluationArgs evaluationArgs, CancellationToken cancellationToken)
                {
                    // DB 접근
                    var handRankingData = CardevilCore.Instance.Database.Database.HandRankingDataList
                        .FirstOrDefault(d => d.Ranking == _handRanking);
                    Debug.Assert(handRankingData != null, $"HandRanking Data: {_handRanking} is null.");

                    // 족보 기본 데미지 이벤트 내용
                    int damageAmount = handRankingData.Value;
                    await EvaluationView.Current.DoStep(damageAmount, EvaluationView.EvaluationType.Plus);
                    // TODO:
                    // 족보 관련해서 수정 유물 등이 있다면 새로운 args 정의,
                    // 호출해야함.
                    
                    evaluationArgs.AddDamage(damageAmount);
                }
            }
            
            // 카드 기본 데미지
            _attackCardsUsingIndex = 0;
            for (int i = 0; i < _toUseCards.Length; i++)
            {
                if (!_toUseCards[i]) continue;
                
                int priority = (int)CardDamageEvaluationArgs.Order.PlusCardDamage;
                queue.Enqueue(priority, AddCardDamageAsync);
            }
            return;
            
            async UniTask AddCardDamageAsync(CardDamageEvaluationArgs evaluationArgs, CancellationToken cancellationToken)
            {
                int damageAmount = _toUseCards[_attackCardsUsingIndex++].Data.FinalNumber;
                await EvaluationView.Current.DoStep(damageAmount, EvaluationView.EvaluationType.Plus);
                // TODO:
                // 카드별 기본 데미지 유물 호출을 위해 새로운 args 정의,
                // 호출해야함.
                
                evaluationArgs.AddDamage(damageAmount);
            }
        }
        
        // 데미지 평가 전 View를 정리함.
        private async UniTask ClearView(CardDamageEvaluationArgs args, CancellationToken cancellationToken)
        {
            await EvaluationView.Current.ClearAllTextAsync();
        }

        // '이동 카드 하나'의 처리가 끝난 시점의 플레이어 위치를 등록함.
        private async UniTask OnPlayerMovingEnded(PlayerMoveArgs args, CancellationToken token)
        {
            _playerPosition = args.PlayerPosition;
        }

        // 모든 데미지 평가 과정이 끝났을 때 해당 스냅샷을 Model에 등록함.
        private async UniTask OnEvaluationEnded(CardDamageEvaluationArgs args, CancellationToken token)
        {
            // TODO: 데미지 float -> int 시점 확인
            var cardsData = args.Cards.GetCardsData();
            var result = new EvaluationResult(cardsData, (int)args.Damage, args.HandRanking, _playerPosition);
            _model.AddResult(result);
        }
        
        public EvaluationResult? GetCurrentEvaluationResult()
        {
            return _model.CurrentResult;
        }
    }
}