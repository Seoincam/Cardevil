namespace Cardevil.Cards.Data.Modifiers.Number
{
    /// <summary>
    /// 카드의 데미지 배율을 증가시키는 Modifier.
    /// </summary>
    public sealed class DamageModifier : IModifier
    {
        public ModifierType Type => ModifierType.AttackDamage;

        private readonly float _damage;

        /// <summary>
        /// 증가시킬 데미지 배율을 지정하여 초기화.
        /// </summary>
        /// <param name="damage">추가할 데미지 배율</param>
        public DamageModifier(float damage)
        {
            _damage = damage;
        }

        public void Apply(BuiltCardData.Builder b)
        {
            b.AddDamageMultiplier(_damage);
        }
    }
}