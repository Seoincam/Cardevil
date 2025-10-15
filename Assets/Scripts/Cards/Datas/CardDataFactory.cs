using Cardevil.Utils.Directions;
using System;
using System.Collections.Generic;

namespace Cardevil.Cards.Data
{
    // 카드 원본 데이터 생성기
    public class CardDataFactory
    {
        public List<CardData> datas;

        public void Init()
        {
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
                    
                    datas.Add(new CardData(id++, number));
                }

                // 오망성 Number Data
                number = new NumberModifierPipeline();
                number.Add(new ColorModifier(color));
                for (int i = 0; i < 9; i++) 
                    number.Add(new SelectableNumberModifier());
                
                datas.Add(new CardData(id++, number));
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
                    
                    datas.Add((new CardData(id++, move)));
                }
            }
            
            // 4방향 선택 가능 Move Data
            for (int i = 0; i < 2; i++)
            {
                move = new MoveModifierPipeline();
                for (int j = 0; j < 4; j++)
                    move.Add((new SelectableDirectionModifier()));
            
                datas.Add(new CardData(id++, move));
            }
        }
    }
}