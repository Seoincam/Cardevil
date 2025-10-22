using Cardevil.Utils.Directions;

namespace Cardevil.Cards.Data.Modifiers.Move
{
    /// <summary>
    /// 선택 가능한 방향 슬롯에 실제 방향 값을 확정하는 Modifier.  
    /// - 슬롯이 2개인 경우: 첫 번째 값이 정해져 있으면 두 번째를 반대 방향으로 자동 확정  
    /// </summary>
    public sealed class DirSelectableModifier : IModifier
    {
        /// <inheritdoc/>
        public ModifierType Type => ModifierType.MoveDirSelectable;

        private Direction? _direction;

        public DirSelectableModifier(Direction? direction = null)
        {
            _direction = direction;
        }
        
        /// <inheritdoc/>
        public void Apply(BuiltCardData.Builder b)
        {
            if (b.DirectionSelectables.Count == 1 && _direction.HasValue)
            {
                b.AddDirectionSelectable(_direction);
            }
            
            // 두 개의 슬롯이 있고, 첫 번째가 확정된 경우: 반대 방향으로 자동 확정
            if (b.DirectionSelectables.Count == 2 && b.DirectionSelectables[0].HasValue && !b.DirectionSelectables[1].HasValue)
            {
                b.AddDirectionSelectable(b.DirectionSelectables[0]!.Value.Opposite());
            }
        }
    }
}