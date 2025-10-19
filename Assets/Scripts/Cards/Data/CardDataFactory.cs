using Cardevil.Cards.Data.Modifiers.Move;
using Cardevil.Cards.Data.Modifiers.Number;
using Cardevil.Utils.Directions;
using System;
using System.Collections.Generic;

namespace Cardevil.Cards.Data
{
    /// <summary>
    /// 카드의 기본 원본 데이터(50장)를 생성하는 팩토리 클래스.
    /// 숫자(Number) 카드와 이동(Move) 카드를 모두 생성.
    /// </summary>
    public static class CardDataFactory
    {
        public static List<CardPipeline> CreateBaseData()
        {
            List<CardPipeline> data = new();
            int id = 0;
            
            // Number Data 생성
            NumberModifierPipeline number;
            foreach (CardColor color in Enum.GetValues(typeof(CardColor)))
            {
                if (color == CardColor.None) continue;
                
                // 일반 Number Data (2~10)
                for (int i = 2; i <= 10; i++)
                {
                    number = new NumberModifierPipeline();
                    number.Add(new ColorModifier(color));
                    number.Add(new SelectableNumberModifier());
                    number.Add(new SelectableNumberConfirmModifier(i));
                    
                    data.Add(new CardPipeline(id++, number));
                }

                // 오망성 Number Data
                number = new NumberModifierPipeline();
                number.Add(new ColorModifier(color));
                for (int i = 0; i < 9; i++) 
                    number.Add(new SelectableNumberModifier());
                
                data.Add(new CardPipeline(id++, number));
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
                    move.Add(new SelectableDirectionModifier());
                    move.Add(new SelectableDirectionConfirmModifier(direction));
                    
                    data.Add((new CardPipeline(id++, move)));
                }
            }
            
            // 4방향 선택 가능 Move Data
            for (int i = 0; i < 2; i++)
            {
                move = new MoveModifierPipeline();
                for (int j = 0; j < 4; j++)
                    move.Add((new SelectableDirectionModifier()));
            
                data.Add(new CardPipeline(id++, move));
            }

            return data;
        }
    }
}