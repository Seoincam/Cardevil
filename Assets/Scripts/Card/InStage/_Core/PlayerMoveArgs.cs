using Cardevil.Events.ExecEvents;
using Cardevil.Utils.Directions;

namespace Cardevil.Card.InStage
{
    /// <summary>
    /// 이동 카드 사용 시 발행되는 이벤트의 인자.
    /// 이동 카드 '하나하나'마다 호출됨
    /// </summary>
    public class PlayerMoveArgs : ExecEventArgs<PlayerMoveArgs>
    {
        public Direction Direction { get; private set; }

        public static PlayerMoveArgs Get(Direction direction)
        {
            var args = Get();
            args.Direction = direction;
            
            return args;
        }

        public override void Clear()
        {
            base.Clear();
            Direction = Direction.None;
        }
    }
}