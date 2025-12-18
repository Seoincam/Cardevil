using Cardevil.Cards.Data;
using Cardevil.Cards.Data.InStage;
using Cardevil.Cards.Visual.Base;
using Cardevil.Cards.Visual.StateMachine;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Cardevil.Cards.Visual.Controller
{
    public abstract class CardVisualControllerBase : MonoBehaviour
    {
        protected CardVisualBase visual;

        public abstract void Init(CardVisualBase visualBase, CardData data);
        
        protected CardVisualSpriteSet ConfigureSpriteSet(CardData data)
        {
            return data.Kind switch
            {
                CardKind.Attack => UpdateAttackData(data),
                CardKind.Move => UpdateMoveData(data),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
        
        // TODO 강화 데이터 핸들링
        private CardVisualSpriteSet UpdateAttackData(CardData data)
        {
            Sprite innerFrame = CardSpriteCache.GetInnerFrame(data.Color);
            List<Sprite> sprites = new();
            Sprite small = null;

            var n = data.NumberSelectState;
            
            if (n.FinalValue.HasValue)
            {
                sprites.Add(CardSpriteCache.GetNumber(data.Color, n.FinalValue.Value));
                small = CardSpriteCache.GetSmallNumber(data.Color, n.FinalValue.Value);
                return new CardVisualSpriteSet(innerFrame, sprites, small);
            }

            // 오망성인 경우 따로 분류
            if (n.Selectables.Count == 9)
            {
                sprites.Add(CardSpriteCache.GetStar(data.Color));
                small = CardSpriteCache.GetSmallStar(data.Color);
            }
            else
            {
                foreach (var item in n.Selectables)
                {
                    sprites.Add(item.hasValue
                        ? CardSpriteCache.GetNumber(data.Color, item.value)
                        : CardSpriteCache.GetQuestionMark(data.Color));
                }
            }
            
            return new CardVisualSpriteSet(innerFrame, sprites, small);
        }

        private CardVisualSpriteSet UpdateMoveData(CardData data)
        {
            Sprite innerFrame = null;
            List<Sprite> sprites = new();

            var d = data.DirectionSelectState;

            if (d.FinalValue.HasValue)
            {
                innerFrame = CardSpriteCache.GetInnerFrame(d.FinalValue.Value);
                sprites.Add(CardSpriteCache.GetArrow(d.FinalValue.Value));
                return new CardVisualSpriteSet(innerFrame, sprites);
            }

            innerFrame = CardSpriteCache.GetInnerFrame(data.DirectionFlag);
            sprites.Add(CardSpriteCache.GetArrow(data.DirectionFlag));
            
            return new CardVisualSpriteSet(innerFrame, sprites);
        }
    }
}