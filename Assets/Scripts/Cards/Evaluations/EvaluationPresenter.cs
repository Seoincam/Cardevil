using Cardevil.Cards.Data;
using Cardevil.Cards.InStage.Model;
using Cardevil.Cards.InStage.Presenter;
using Cardevil.Utils;
using Cardevil.Utils.Directions;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Cardevil.Cards.Evaluations
{
    public interface IEvaluationPresenter
    {
        void ClearHandRankingText();
        void UpdateHandRankingText(IEnumerable<Card> cards);
        void ConfigureSequence(IEnumerable<Card> sortedCards);
        UniTask InvokeSequence();
    }
    
    public class EvaluationPresenter : IEvaluationPresenter
    {
        private EvaluationResultsModel _model;
        private EvaluationView _view;
        private EvaluationSequenceFactory _factory;
        
        private EvaluationSequence _seq;
        private EvaluationResult.Builder _resultBuilder;

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
            var go = Managers.Resource.Instantiate(path, canvas).gameObject;
            if (!go)
            {
                LogEx.LogError($"Evaluation UI Animator가 존재하지 않음! path: {path}");
                return;
            }
            _view = go.GetComponent<EvaluationView>();
        }
        
        // 족보 표시 텍스트 초기화()
        public void ClearHandRankingText()
        {
            _view.UpdateHandRankingText(HandRanking.None);
        }
        
        // 족보 표시 텍스트 설정(선택한 카드 목록)
        public void UpdateHandRankingText(IEnumerable<Card> selection)
        {
            var handRanking = HandRankingEvaluator.EvaluateHandRanking(selection);
            _view.UpdateHandRankingText(handRanking);
        }

        // sequence 구성(정렬된 선택한 카드 목록)
        public void ConfigureSequence(IEnumerable<Card> sortedCards)
        {
            var handRanking = HandRankingEvaluator.EvaluateHandRanking(sortedCards);
            
            List<Direction> moves = new();
            foreach (var card in sortedCards)
            {
                if (card.Data.Kind != CardKind.Move)
                    continue;
                if (card.Data.DirectionSelectState.FinalValue != null)
                    moves.Add((Direction)card.Data.DirectionSelectState.FinalValue);
            }

            _resultBuilder = EvaluationResult.CreateBuilder()
                .SetHandRanking(handRanking)
                .SetMoves(moves);
            
            _seq = _factory.ConfigureSequence(sortedCards);
        }

        // sequence 실행 / await 가능해야함
        public async UniTask InvokeSequence()
        {
            _seq.Build();
            _view.Clear();
            
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
                    _view.RegisterStep(step);
                    
                    if (i != stepGroup.Count - 1)
                        await UniTask.Delay(TimeSpan.FromSeconds(.3f));
                }
                
                await _view.DoStep(totalDamage);
                await UniTask.Delay(TimeSpan.FromSeconds(.2f));
            }

            var result = _resultBuilder
                .SetDamage((int)Math.Round(totalDamage))
                .Build();
            _model.Add(result);
        }
    }
}