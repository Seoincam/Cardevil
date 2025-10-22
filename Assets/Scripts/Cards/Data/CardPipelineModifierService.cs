using Cardevil.Cards.Data.Modifiers;
using System;

namespace Cardevil.Cards.Data
{
    public class CardPipelineModifierService
    {
        private CardLibrary _library;
        
        public void Init(CardLibrary library)
        {
            _library = library;
        }
        
        public void Enhance(int id, ModifierType type, int count = 1)
        {
            var pipeline = _library.GetPipelineById(id);
            
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
        }
    }
}