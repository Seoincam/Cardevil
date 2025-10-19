using Cardevil.Cards.Data.Modifiers.Move;
using Cardevil.Cards.Data.Modifiers.Number;
using Cardevil.Utils.Directions;
using System;
using System.Collections.Generic;

namespace Cardevil.Cards.Data
{
    public static class CardLibraryInitializeExtensions
    {
        /// <summary>
        /// 기본 카드 파이프라인(50개)을 생성하는 확장 메서드.
        /// </summary>
        public static void CreateBasePipelines(this HashSet<CardPipeline> pipelines, CardDataModifierService modifierService)
        {
            pipelines.Clear();
            int id = 0;
            
            // Number Data 생성
            foreach (CardColor color in Enum.GetValues(typeof(CardColor)))
            {
                NumberModifierPipeline number;
                if (color == CardColor.None) continue;
                
                // 일반 Number Data (2~10)
                for (int i = 2; i <= 10; i++)
                {
                    number = new NumberModifierPipeline();
                    
                    // Modifier 추가
                    number.AddModifier(new ColorModifier(color));
                    number.AddModifier(new SelectableNumberModifier());
                    number.AddModifier(new SelectableNumberConfirmModifier(i));
                    
                    number.SetPossibleEnhancements
                    (
                        modifierService.GetBaseSelectableEnhancement(),
                        modifierService.GetBaseDamageEnhancement()
                    );
                    
                    pipelines.Add(new CardPipeline(id++, number));
                }

                // 오망성 Number Data
                number = new NumberModifierPipeline();
                number.AddModifier(new ColorModifier(color));
                for (int i = 0; i < 9; i++) 
                    number.AddModifier(new SelectableNumberModifier());
                
                pipelines.Add(new CardPipeline(id++, number));
            }
            
            // Move Data 생성
            MoveModifierPipeline move;
            foreach (Direction direction in Enum.GetValues(typeof(Direction)))
            {
                if (direction == Direction.None) continue;
                
                // 일반 Move 
                for (int i = 0; i < 2; i++)
                {
                    move = new MoveModifierPipeline();
                    
                    // Modifier 추가
                    move.AddModifier(new SelectableDirectionModifier());
                    move.AddModifier(new SelectableDirectionConfirmModifier(direction));
                    
                    // 강화 가능성 추가
                    move.SetPossibleEnhancements
                    (
                        modifierService.GetBaseMoveEnhancement()
                    );
                    
                    pipelines.Add((new CardPipeline(id++, move)));
                }
            }
            
            // 4방향 선택 가능 Move Data
            for (int i = 0; i < 2; i++)
            {
                move = new MoveModifierPipeline();
                for (int j = 0; j < 4; j++)
                    move.AddModifier((new SelectableDirectionModifier()));
            
                pipelines.Add(new CardPipeline(id++, move));
            }
        }
    }
}