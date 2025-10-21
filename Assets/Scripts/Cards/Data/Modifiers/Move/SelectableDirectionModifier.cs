namespace Cardevil.Cards.Data.Modifiers.Move
{
    /// <summary>
    /// 이동 카드에 선택 가능한 방향을 하나 추가하는 Modifier.
    /// </summary>
    public sealed class SelectableDirectionModifier : IModifier
    {
        /// <inheritdoc/>
        public ModifierType Type => ModifierType.MoveDirSelectable;

        /// <inheritdoc/>
        public void Apply(BuildCardContext ctx)
        {
            // ctx.Selectables.Add(null);
        }
    }
}