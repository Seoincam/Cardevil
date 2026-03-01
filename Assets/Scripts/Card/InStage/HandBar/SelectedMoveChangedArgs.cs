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