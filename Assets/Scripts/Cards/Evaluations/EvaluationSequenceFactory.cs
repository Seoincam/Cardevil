using Cardevil.Cards.Data;
using Cardevil.Cards.InStage.Model.ReadOnly;
using Cardevil.Cards.InStage.Presenter;
using Cardevil.Relics.OnEvaluation;
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
            if (selection == null) return seq;

            // 분류
            var attackCards = new List<Card>();
            var moveCards = new List<Card>();
            foreach (var c in selection)
                (c.Data.Kind == CardKind.Attack ? attackCards : moveCards).Add(c);

            // Move Only
            if (attackCards.Count == 0 && moveCards.Count > 0)
            {
                List<IEvaluateVisual> visuals = new();
                foreach (var card in moveCards)
                    visuals.Add(card.EvaluateVisual);
                
                seq.Append(EvaluationStep.Get()
                    .SetValue(EvaluationStep.Type.Move)
                    .SetVisual(visuals));

                return seq;
            }

            // 유물 버킷
            var perCardBonus = new List<IRelicEffectOnEvaluation>(2);
            var plusEffects = new List<IRelicEffectOnEvaluation>();
            var mulEffects = new List<IRelicEffectOnEvaluation>();

            
            foreach (var owned in Managers.Relic.OwnedRelics)
            foreach (var eff in owned.Relic.Effects)
            {
                if (eff is not IRelicEffectOnEvaluation onEval)
                    continue;
                switch (onEval)
                {
                    case DamageOnEachCardEffect perCard: perCardBonus.Add(perCard); break;
                    default: (onEval.IsPlus ? plusEffects : mulEffects).Add(onEval); break;
                }
            }
            
            // 족보
            var handRanking = HandRankingEvaluator.EvaluateHandRanking(attackCards, out var inRankCards);
            // TODO: db 접근 처음에 일괄적으로 하도록 바꾸기
            var data = Managers.Database.Database.HandRankingDataList
                .FirstOrDefault(d => d.Ranking == handRanking);
            if (data == null)
                LogEx.LogWarning($"Database에 족보 데이터가 존재하지 않음! : {handRanking}");
            
            // 기본 족보 보너스
            if (data != null && handRanking > HandRanking.High)
            {
                List<IEvaluateVisual> visuals = new();
                foreach (var card in inRankCards)
                    visuals.Add(card.EvaluateVisual);
                
                seq.Append(EvaluationStep.Get()
                    .SetValue(EvaluationStep.Type.Plus, data.Value)
                    .SetVisual(visuals)); // 족보에 포함되는 카드들만 추가함
            }
            
            // 기본 데미지 + 보너스 데미지
            if (handRanking == HandRanking.High)
            {
                Card top = attackCards[0];
                float topVal = (int)top.Data.NumberSelectState.FinalValue;
                for (int i = 1; i < attackCards.Count; i++)
                {
                    int v = (int)attackCards[i].Data.NumberSelectState.FinalValue;
                    if (v > topVal) { top = attackCards[i]; topVal = v; }
                }
                
                seq.Append(EvaluationStep.Get()
                    .SetValue(EvaluationStep.Type.Plus, topVal)
                    .SetVisual(top.EvaluateVisual));
                
                // 추가 데미지 유물 효과 적용
                foreach (var effect in perCardBonus)
                    seq.Join(effect.MakeEvaluationStep());
            }
            else if (handRanking != HandRanking.None && inRankCards?.Count > 0)
            {
                foreach (var card in inRankCards)
                {
                    float v = (int)card.Data.NumberSelectState.FinalValue;
                    
                    seq.Append(EvaluationStep.Get()
                        .SetValue(EvaluationStep.Type.Plus, v)
                        .SetVisual(card.EvaluateVisual));
                    
                    // 추가 데미지 유물 효과 적용
                    foreach (var effect in perCardBonus)
                        seq.Join(effect.MakeEvaluationStep());
                }    
            }

            // 유물 효과 적용
            foreach (var effect in plusEffects)
            {
                if (effect.CanTrigger(handRanking, _model))
                    seq.Append(effect.MakeEvaluationStep());
            }

            foreach (var effect in mulEffects)
            {
                if (effect.CanTrigger(handRanking, _model))
                    seq.Append(effect.MakeEvaluationStep());
            }

            return seq;
        }
    }
}