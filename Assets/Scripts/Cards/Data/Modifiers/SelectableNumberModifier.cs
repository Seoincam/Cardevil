using Cardevil.Cards.Data.InStage;

namespace Cardevil.Cards.Data.Modifiers
{
    /// <summary>
    /// 카드에 선택 가능한 미정 값을 추가하는 Modifier.  
    /// </summary>
    public sealed class SelectableNumberModifier : IModifier
    {
        public ModifierType Type => ModifierType.AttackNumSelectable;

        public void Apply(CardData.Builder b)
        {
            b.AddNumberSelectable(null);
        }
    }
}