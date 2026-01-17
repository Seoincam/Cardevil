using Cardevil.Cards.Core;
using System;

namespace Cardevil.Cards.Utils
{
    public class CardSpecModifierService
    {
        private CardStatus _status;
        
        public CardSpecModifierService(CardStatus status)
        {
            _status = status;
        }

        public void Enhance(int specId, Guid enhancementId, ModifierType type, int count, Guid nextEnhancementId)
        {
            var cardSpec = _status.GetSpecById(specId);
            
            for (int i = 0; i < count; i++)
            {
                // TODO: Factory 클래스를 만드는 것도 고려하기
                switch (type)
                {
                    case ModifierType.AttackNumSelectable:
                        cardSpec.AddModifier(new SelectableNumberModifier());
                        break;
                    
                    case ModifierType.AttackDamage:
                        cardSpec.AddModifier(new DamageModifier());
                        break;
                    
                    case ModifierType.MoveDirSelectable:
                        cardSpec.AddModifier(new DirSelectableModifier());
                        break;
                    
                    default:
                        throw new InvalidOperationException();
                }
            }
            
            // 현재 강화 단계로 갱신
            cardSpec.SetCurrentEnhancementId(enhancementId);
            
            // 가능한 다음 강화 단계 업데이트
            if (nextEnhancementId == Guid.Empty)
                cardSpec.ClearNextEnhancementIds();
            else 
                cardSpec.SetCurrentEnhancementId(nextEnhancementId);
            
            // 데이터 갱신
            _status.UpdateDataMap(specId);
        }
    }
}