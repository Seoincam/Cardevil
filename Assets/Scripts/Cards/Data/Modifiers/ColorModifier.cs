using Cardevil.Cards.Data.InStage;

namespace Cardevil.Cards.Data.Modifiers
{
    /// <summary>
    /// 카드의 색상을 변경하는 Modifier.
    /// </summary>
    public sealed class ColorModifier : IModifier
    {
        public ModifierType Type => ModifierType.AttackColor;

        private readonly CardColor _color;

        /// <summary>
        /// 지정된 색상으로 설정.
        /// 여러 개가 있을 경우 가장 마지막에 지정한 색상으로 설정.
        /// </summary>
        /// <param name="color">적용할 카드 색상</param>
        public ColorModifier(CardColor color)
        {
            _color = color;
        }

        public void Apply(CardData.Builder b)
        {
            b.SetColor(_color);
        }
    }
}