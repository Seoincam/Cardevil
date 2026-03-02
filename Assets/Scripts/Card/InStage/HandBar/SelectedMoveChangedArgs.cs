using Cardevil.Events.ExecEvents;
using Cardevil.Utils.Directions;
using System.Collections.Generic;

namespace Cardevil.Card.InStage
{
    /// <summary>
    /// 선택한 이동 카드가 바뀔 때마다 발행되는 이벤트의 인자.
    /// </summary>
    public class SelectedMoveChangedArgs : ExecEventArgs<SelectedMoveChangedArgs>
    {
        /// <summary>
        /// 선택된 이동 카드가 존재하고, 모든 이동 카드의 값이 선택됐다면 <c>true</c>를 반환.
        /// </summary>
        public bool ShouldShow { get; private set; }
        public IReadOnlyList<Direction> Directions { get; private set; }

        public static SelectedMoveChangedArgs Get(bool shouldShow, IReadOnlyList<Direction> directions)
        {
            var args = Get();
            args.ShouldShow = shouldShow;
            args.Directions = directions;
            
            return args;
        }

        public override void Clear()
        {
            base.Clear();
            ShouldShow = false;
            Directions = null;
        }
    }
}