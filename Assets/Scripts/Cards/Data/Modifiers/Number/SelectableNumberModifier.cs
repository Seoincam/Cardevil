namespace Cardevil.Cards.Data.Modifiers.Number
{
    /// <summary>
    /// 카드에 선택 가능한 미정 값을 추가하는 Modifier.  
    /// </summary>
    public sealed class SelectableNumberModifier : IModifier
    {
        public ModifierType Type => ModifierType.AttackNumSelectable;

        public void Apply(BuiltCardData.Builder b)
        {
            b.AddNumberSelectable(null);
        }
    }
}