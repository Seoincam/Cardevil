using Cardevil.Cards.Data;
using Cardevil.Cards.Data.InStage;
using Cardevil.Cards.InStage.Model.ReadOnly;
using Cardevil.Cards.InStage.Presenter;
using System.Collections.Generic;
using System.Linq;
using Cardevil.Utils;

namespace Cardevil.Cards.Evaluations
{
    public class EvaluationSequenceFactory
    {
        private readonly IReadOnlyEvaluationResultsModel _model;
        
        public EvaluationSequenceFactory(IReadOnlyEvaluationResultsModel model)
        {
            _model = model;
        }
        
        public EvaluationSequence ConfigureSequence(IEnumerable<Card> selection)
        {
            EvaluationSequence seq = EvaluationSequence.Get();
            
            List<Card> attackCards = selection.Where(c => c.Data.Kind == CardKind.Attack).ToList();
            List<Card> moveCards = selection.Where(c => c.Data.Kind == CardKind.Move).ToList();
            List<CardData> moves = moveCards.Select(c => c.Data).ToList();
            
            // Move Only
            if (attackCards.Count == 0 && moveCards.Count > 0)
            {
                seq.Append(
                    EvaluationStep.Get()
                    .SetValue(EvaluationStepType.Move)
                    .SetVisual(moveCards));

                // _pending = new EvaluationResult(moves);
                return seq;
            }
            
            // 족보 계산
            HandRanking handRanking = HandRankingEvaluator.EvaluateHandRanking(attackCards, out var cardsInHandRanking);
            // _pending = new EvaluationResult(moves, handRanking);
            
            // 기본 족보 보너스
            if (handRanking > HandRanking.High)
            {
                // TODO: db 접근 처음에 일괄적으로 하도록 바꾸기
                var data = Managers.Database.Database.HandRankingDataList
                    .FirstOrDefault(d => d.Ranking == handRanking);

                if (data == null)
                {
                    LogEx.LogError($"Database에 족보 데이터가 존재하지 않음! : {handRanking}");
                    return seq;
                }

                seq.Append(EvaluationStep.Get()
                    .SetValue(EvaluationStepType.Plus, data.Value)
                    .SetVisual(cardsInHandRanking)); // 족보에 포함되는 카드들만 추가함
            }

            // 기본 데미지
            if (handRanking == HandRanking.High)
            {
                var top = attackCards.Aggregate((best, cur) =>
                    cur.Data.NumberSelectState.FinalValue > best.Data.NumberSelectState.FinalValue ? cur : best);
                seq.Append(EvaluationStep.Get()
                    .SetValue(EvaluationStepType.Plus, (float)top.Data.NumberSelectState.FinalValue)
                    .SetVisual(top));
                // 추가 데미지
            }
            else if (handRanking != HandRanking.None)
            {
                foreach (var card in cardsInHandRanking)
                {
                    seq.Append(EvaluationStep.Get()
                        .SetValue(EvaluationStepType.Plus, (float)card.Data.NumberSelectState.FinalValue)
                        .SetVisual(card));
                    // 추가 데미지
                }    
            }
            
            // Plus 유물
            
            // Multiply 유물

            return seq;
        }
    }
}