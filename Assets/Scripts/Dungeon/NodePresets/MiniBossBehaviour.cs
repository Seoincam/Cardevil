using Cardevil.Dungeon;
using Cardevil.Dungeon.Core;
using Cardevil.Utils;
using UnityEngine;

namespace Cardevil.Dugeon.NodePresets
{
    public class MiniBossBehaviour : DungeonNodeBehaviour
    {
        public override void OnEnter()
        {
            LogEx.Log("Mini Boss Node Entered");
        }

        public override void OnExit(NodeClearInfo clearInfo)
        {
            LogEx.Log("Mini Boss Node Exited");
        }
    }
}