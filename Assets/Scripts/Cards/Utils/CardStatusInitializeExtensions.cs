using Cardevil.Cards.Core;
using Cardevil.Cards.Enhancements;
using Cardevil.Utils.Directions;
using System;

namespace Cardevil.Cards.Utils
{
    public static class CardStatusInitializeExtensions
    {
        /// <summary>
        /// 기본 카드 스펙(50개)을 생성하는 확장 메서드.
        /// </summary>
        public static void CreateBaseSpec(this CardStatus cardStatus, EnhancementDataLibrary enhancement)
        {
            // 테스트용 플래그
            bool isEnhancement = enhancement != null;
            
            cardStatus.specMap.Clear();
            int id = 0;
            StageCardSpec spec;
            
            // Number Data 생성
            foreach (CardColor color in Enum.GetValues(typeof(CardColor)))
            {
                if (color == CardColor.None) continue;
                
                // 일반 Number Data (2~10)
                for (int i = 2; i <= 10; i++)
                {
                    spec = new StageCardSpec(CardKind.Attack, id);
                    
                    // Modifier 추가
                    spec.AddModifier(new SelectableColorModifier());
                    spec.AddModifier(new SelectableColorConfirmModifier(color));
                    spec.AddModifier(new SelectableNumberModifier());
                    spec.AddModifier(new SelectableNumberConfirmModifier(i));

                    if (isEnhancement)
                    {
                        // 강화 상태 설정
                        spec.SetCurrentEnhancementId(Guid.Empty);
                        
                        // 강화 가능성 추가
                        spec.SetNextEnhancementIds
                        (
                            enhancement.GetId(ModifierType.AttackNumSelectable, 1),
                            enhancement.GetId(ModifierType.AttackDamage, 1)
                        );
                    }
                    
                    cardStatus.specMap[id++] = spec;
                }

                // 오망성 Number Data
                spec = new StageCardSpec(CardKind.Attack, id);
                spec.AddModifier(new SelectableColorModifier());
                spec.AddModifier(new SelectableColorConfirmModifier(color));
                for (int i = 0; i < 9; i++) 
                    spec.AddModifier(new SelectableNumberModifier());

                if (isEnhancement)
                {
                    // 강화 상태 설정
                    spec.SetCurrentEnhancementId
                    (
                        enhancement.GetId(ModifierType.AttackNumSelectable, 3)
                    );
                }

                cardStatus.specMap[id++] = spec;
            }
            
            // Move Data 생성
            foreach (Direction direction in Enum.GetValues(typeof(Direction)))
            {
                if (direction == Direction.None) continue;
                
                // 일반 Move 
                for (int i = 0; i < 2; i++)
                {
                    spec = new  StageCardSpec(CardKind.Move, id);

                    // Modifier 추가
                    spec.AddModifier(new DirSelectableModifier(direction));
                    
                    if (isEnhancement)
                    {
                        // 강화 상태 설정
                        spec.SetCurrentEnhancementId(Guid.Empty);
                    
                        // 강화 가능성 추가
                        spec.SetNextEnhancementIds
                        (
                            enhancement.GetId(ModifierType.MoveDirSelectable, 1)
                        );
                    }
                    
                    cardStatus.specMap[id++] = spec;
                }
            }
            
            // 4방향 선택 가능 Move Data
            for (int i = 0; i < 2; i++)
            {
                spec = new StageCardSpec(CardKind.Move, id);
                
                // Modifier 추가
                for (int j = 0; j < 4; j++)
                    spec.AddModifier(new DirSelectableModifier());

                if (isEnhancement)
                {
                    // 강화 상태 설정
                    spec.SetCurrentEnhancementId(enhancement.GetId(ModifierType.MoveDirSelectable, 2));
                }
                
                cardStatus.specMap[id++] = spec;
            }
        }
    }
}