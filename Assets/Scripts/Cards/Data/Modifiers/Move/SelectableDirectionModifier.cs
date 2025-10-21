namespace Cardevil.Cards.Data.Modifiers.Move
{
    /// <summary>
    /// 이동 카드에 선택 가능한 방향을 하나 추가하는 Modifier.
    /// </summary>
    public sealed class SelectableDirectionModifier : IMoveModifier
    {
        /// <inheritdoc/>
        public MoveModifierType Type => MoveModifierType.Selectable;

        /// <inheritdoc/>
        public void Apply(ref MoveBuildContext ctx)
        {
            ctx.Selectables.Add(null);
        }
    }
}