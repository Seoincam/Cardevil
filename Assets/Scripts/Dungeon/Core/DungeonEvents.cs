using Cardevil.Core.Events.ExecEvent;

namespace Cardevil.Dungeon
{
    /// <summary>
    /// 노드에 진입했을 때 발생하는 이벤트 인자
    /// </summary>
    public class NodeEnteredEventArgs : ExecEventArgs<NodeEnteredEventArgs>
    {
        public DungeonNode Node { get; private set; }

        public void Init(DungeonNode node)
        {
            Node = node;
        }

        public override void Clear()
        {
            base.Clear();
            Node = null;
        }
    }

    /// <summary>
    /// 노드에서 나갈 때 발생하는 이벤트 인자
    /// </summary>
    public class NodeExitedEventArgs : ExecEventArgs<NodeExitedEventArgs>
    {
        public DungeonNode Node { get; private set; }
        public NodeExitInfo ExitInfo { get; private set; }

        public void Init(DungeonNode node, NodeExitInfo exitInfo)
        {
            Node = node;
            ExitInfo = exitInfo;
        }

        public override void Clear()
        {
            base.Clear();
            Node = null;
            ExitInfo = default;
        }
    }

    /// <summary>
    /// 던전을 완료했을 때 발생하는 이벤트 인자
    /// </summary>
    public class DungeonCompletedEventArgs : ExecEventArgs<DungeonCompletedEventArgs>
    {
        public Dungeon Dungeon { get; private set; }

        public void Init(Dungeon dungeon)
        {
            Dungeon = dungeon;
        }

        public override void Clear()
        {
            base.Clear();
            Dungeon = null;
        }
    }
}

