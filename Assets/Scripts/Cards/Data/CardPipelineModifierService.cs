using Cardevil.Cards.Data.Modifiers;
using System;
using Unity.VisualScripting;

namespace Cardevil.Cards.Data
{
    public class CardPipelineModifierService
    {
        private CardStatus _status;
        
        public CardPipelineModifierService(CardStatus status)
        {
            _status = status;
        }

        public void Enhance(int pipelineId, Guid enhancementId, ModifierType type, int count, Guid nextEnhancementId)
        {
            var pipeline = _status.GetPipelineById(pipelineId);
            
            for (int i = 0; i < count; i++)
            {
                // TODO: Factory 클래스를 만드는 것도 고려하기
                switch (type)
                {
                    case ModifierType.AttackNumSelectable:
                        pipeline.AddModifier(new SelectableNumberModifier());
                        break;
                    
                    case ModifierType.AttackDamage:
                        pipeline.AddModifier(new DamageModifier());
                        break;
                    
                    case ModifierType.MoveDirSelectable:
                        pipeline.AddModifier(new DirSelectableModifier());
                        break;
                    
                    default:
                        throw new InvalidOperationException();
                }
            }
            
            // 현재 강화 단계로 갱신
            pipeline.SetCurrentEnhancementId(enhancementId);
            
            // 가능한 다음 강화 단계 업데이트
            if (nextEnhancementId == Guid.Empty)
                pipeline.ClearNextEnhancementIds();
            else 
                pipeline.SetCurrentEnhancementId(nextEnhancementId);
            
            // 데이터 갱신
            _status.UpdateDataMap(pipelineId);
        }
    }
}