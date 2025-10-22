using Cardevil.Cards.Data.Modifiers;
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
        public static void CreateBasePipelines(this Dictionary<int, CardPipeline> pipelines)
        {
            pipelines.Clear();
            int id = 0;
            CardPipeline pipeline;
            
            // Number Data 생성
            foreach (CardColor color in Enum.GetValues(typeof(CardColor)))
            {
                if (color == CardColor.None) continue;
                
                // 일반 Number Data (2~10)
                for (int i = 2; i <= 10; i++)
                {
                    pipeline = new CardPipeline(CardKind.Attack, id);
                    
                    // Modifier 추가
                    pipeline.AddModifier(new ColorModifier(color));
                    pipeline.AddModifier(new SelectableNumberModifier());
                    pipeline.AddModifier(new SelectableNumberConfirmModifier(i));
                    
                    // 강화 가능성 추가
                    pipeline.SetPossibleEnhancements
                    (
                        // modifierDatabase.GetBaseSelectableEnhancement(),
                        // modifierDatabase.GetBaseDamageEnhancement()
                    );

                    pipelines[id++] = pipeline;
                }

                // 오망성 Number Data
                pipeline = new CardPipeline(CardKind.Attack, id);
                pipeline.AddModifier(new ColorModifier(color));
                for (int i = 0; i < 9; i++) 
                    pipeline.AddModifier(new SelectableNumberModifier());

                pipelines[id++] = pipeline;
            }
            
            // Move Data 생성
            foreach (Direction direction in Enum.GetValues(typeof(Direction)))
            {
                if (direction == Direction.None) continue;
                
                // 일반 Move 
                for (int i = 0; i < 2; i++)
                {
                    pipeline = new  CardPipeline(CardKind.Move, id);
                    
                    // Modifier 추가
                    pipeline.AddModifier(new DirSelectableModifier(direction));
                    
                    // 강화 가능성 추가
                    pipeline.SetPossibleEnhancements
                    (
                        // modifierDatabase.GetBaseMoveEnhancement()
                    );
                    
                    pipelines[id++] = pipeline;
                }
            }
            
            // 4방향 선택 가능 Move Data
            for (int i = 0; i < 2; i++)
            {
                pipeline = new CardPipeline(CardKind.Move, id);
                for (int j = 0; j < 4; j++)
                    pipeline.AddModifier(new DirSelectableModifier());

                pipelines[id++] = pipeline;
            }
        }
    }
}