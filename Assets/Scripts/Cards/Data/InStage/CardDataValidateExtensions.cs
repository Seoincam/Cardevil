using System;

namespace Cardevil.Cards.Data.InStage
{
    public static class CardDataValidateExtensions
    {
        public static void Validate(this CardData data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            // ID 검사
            if (data.Id < 0 || data.Id >= 50)
                throw new InvalidOperationException($"잘못된 id: {data.Id}");

            // Attack 카드 검사
            if (data.Kind == CardKind.Attack)
            {
                if (data.Color == CardColor.None)
                    throw new InvalidOperationException("Color가 None입니다.");
                if (data.DamageMultiplier < 1f)
                    throw new InvalidOperationException("DamageMultiplier가 1보다 작습니다.");
                if (data.NumberSelectState == null)
                    throw new InvalidOperationException("NumberSelectState가 null입니다.");
            }

            // Move 카드 검사
            if (data.Kind == CardKind.Move)
            {
                if (data.Length < 1)
                    throw new InvalidOperationException("이동 길이가 1보다 작습니다.");
                if (data.DirectionSelectState == null)
                    throw new InvalidOperationException("DirectionSelectState가 null입니다.");
            }
        }
    }
}


