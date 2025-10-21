using Cardevil.Cards.Data;

namespace Cardevil.Cards.Data.Modifiers.Number
{
    /// <summary>
    /// 카드의 색상을 변경하는 Modifier.
    /// </summary>
    public sealed class ColorModifier : INumberModifier
    {
        public NumberModifierType Type => NumberModifierType.Color;

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

        public void Apply(ref NumberBuildContext ctx)
        {
            ctx.Color = _color;
        }
    }
}