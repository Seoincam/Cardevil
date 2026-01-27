using Cardevil.Cards.Core;
using System;

namespace Cardevil.Cards.Visual
{
    /// <summary>
    /// CardData를 기반으로 <see cref="CardSpriteSet"/>을 구성하는 유틸.
    /// </summary>
    public static class CardSpriteSetConfigurationUtil
    {
        /// <summary>
        /// CardData 타입에 따라 적절한 스프라이트 세트 반환
        /// </summary>
        /// <param name="cardData">구성할 카드 데이터</param>
        /// <returns>Attack/Move 타입에 맞는 스프라이트 세트</returns>
        /// <exception cref="ArgumentOutOfRangeException">Attack/Move 타입이 아닐 경우</exception>
        public static CardSpriteSet Configure(CardData cardData)
        {
            if (cardData.IsAttack) return ConfigureAttack(cardData);
            if (cardData.IsMove) return ConfigureMove(cardData);
            
            throw new ArgumentOutOfRangeException();
        }

        private static CardSpriteSet ConfigureAttack(CardData data)
        {
            var innerFrame = CardSpriteCache.GetInnerFrame(data.FinalColor);
            
            // 값이 하나거나, 선택이 완료됐을 경우.
            if (data.CompleteSelectingValue)
            {
                return CardSpriteSet.SingleWithSmall(
                    innerFrame,
                    mainSprite: CardSpriteCache.GetNumber(data.FinalColor, data.FinalNumber),
                    smallNumber: CardSpriteCache.GetSmallNumber(data.FinalColor, data.FinalNumber));
            }
            
            // 오망성인 경우.
            if (data.IsStar)
            {
                return CardSpriteSet.SingleWithSmall(
                    innerFrame,
                    mainSprite: CardSpriteCache.GetStar(data.FinalColor),
                    smallNumber: CardSpriteCache.GetSmallStar(data.FinalColor));
            }
            
            // 그 외 경우 (값이 여러개고, 선택이 안 된 경우).
            int count = data.SelectableCount;
            var mainSprites = new SpriteKey[count];
            for (int i = 0; i < count; i++)
            {
                var number = data.NumberSelectState.Selectables[i];
                mainSprites[i] = number.hasValue
                    ? CardSpriteCache.GetNumber(data.FinalColor, number.value)
                    : CardSpriteCache.GetQuestionMark(data.FinalColor);
            }
            
            return CardSpriteSet.Multiple(innerFrame, mainSprites);
        }

        private static CardSpriteSet ConfigureMove(CardData data)
        {
            // 값이 하나거나, 선택 완료됐을 경우.
            if (data.CompleteSelectingValue)
            {
                return CardSpriteSet.Single(
                    innerFrame: CardSpriteCache.GetInnerFrame(data.FinalDirection),
                    mainSprite: CardSpriteCache.GetArrow(data.FinalDirection)
                );
            }

            return CardSpriteSet.Single(
                innerFrame: CardSpriteCache.GetInnerFrame(data.DirectionFlag),
                mainSprite: CardSpriteCache.GetArrow(data.DirectionFlag));
        }
    }
}