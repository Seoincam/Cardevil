namespace Cardevil.Cards.Data.Modifiers.Number
{
    /// <summary>
    /// 카드의 데미지 배율을 증가시키는 Modifier.
    /// </summary>
    public sealed class DamageModifier : INumberModifier
    {
        public NumberModifierType Type => NumberModifierType.Damage;

        private readonly float _damage;

        /// <summary>
        /// 증가시킬 데미지 배율을 지정하여 초기화.
        /// </summary>
        /// <param name="damage">추가할 데미지 배율</param>
        public DamageModifier(float damage)
        {
            _damage = damage;
        }

        public void Apply(ref NumberBuildContext ctx)
        {
            ctx.DamageMultiply += _damage;
        }
    }
}