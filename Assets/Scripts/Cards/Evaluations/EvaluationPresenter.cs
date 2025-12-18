using Cardevil.Cards.Data;
using Cardevil.Cards.Data.InStage;
using Cardevil.Cards.InStage.Model;
using Cardevil.Cards.InStage.Model.ReadOnly;
using Cardevil.Cards.InStage.Presenter;
using Cardevil.Utils;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
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
        /// 평가 시퀀스 구성.
        /// 정렬된 카드 목록을 기반으로 족보 및 이동 카드 데이터 설정.
        /// </summary>
        /// <param name="sortedCards">정렬된 카드 목록</param>
        void ConfigureSequence(IEnumerable<Card> sortedCards);
        
        /// <summary>
        /// 평가 시퀀스 실행.
        /// 단계별 평가 및 뷰 연출 처리.
        /// </summary>
        UniTask ExcuteSequenceAsync();
        
        /// <returns>가장 최근의 평과 결과를 반환.</returns>
        EvaluationResult GetCurrentEvaluationResult();
        
        IReadOnlyEvaluationResultsModel ResultsModel { get; }
    }
    
    public class EvaluationPresenter : IEvaluationPresenter
    {
        private EvaluationResultsModel _model;
        private EvaluationView _view;
        private EvaluationSequenceFactory _factory;
        
        private EvaluationSequence _seq;
        private EvaluationResult.Builder _resultBuilder;

        public IReadOnlyEvaluationResultsModel ResultsModel => _model;

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

        public void ConfigureSequence(IEnumerable<Card> sortedCards)
        {
            var handRanking = HandRankingEvaluator.EvaluateHandRanking(sortedCards);

            List<CardData> attacks = sortedCards
                .Where(c => c.Data.Kind == CardKind.Attack)
                .Select(c => c.Data).ToList();

            List<CardData> moves = sortedCards
                .Where(c => c.Data.Kind == CardKind.Move)
                .Select(c => c.Data).ToList();

            _resultBuilder = EvaluationResult.CreateBuilder()
                .SetHandRanking(handRanking)
                .SetAttacks(attacks)
                .SetMoves(moves);
            
            _seq = _factory.ConfigureSequence(sortedCards);
        }

        public async UniTask ExcuteSequenceAsync()
        {
            _seq.Build();
            await _view.Clear();
            
            int index = 0;
            float totalDamage = 0f;
            while (_seq.TryGetStepGroup(index++, out List<EvaluationStep> stepGroup))
            {
                if (stepGroup == null || stepGroup.Count == 0) 
                    continue;
                
                for (int i = 0; i < stepGroup.Count; i++)
                {
                    var step = stepGroup[i];
                    step.CalculateDamage(ref totalDamage);
                    step.ExecuteVisualEffect();
                    _view.RegisterStep(step);
                    
                    await UniTask.Delay(TimeSpan.FromSeconds(.3f));
                }
                
                await _view.DoStep(totalDamage);
                await UniTask.Delay(TimeSpan.FromSeconds(.2f));
            }
            _view.Clear().Forget();
            
            var result = _resultBuilder
                .SetDamage((int)Math.Round(totalDamage))
                .Build();
            _model.Add(result);
        }

        public EvaluationResult GetCurrentEvaluationResult()
        {
            return _model.CurrentResult;
        }
    }
}