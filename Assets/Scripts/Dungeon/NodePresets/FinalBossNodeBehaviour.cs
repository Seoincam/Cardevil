using Cardevil.Dungeon;
using Cardevil.Dungeon.Core;
using UnityEngine;

namespace Cardevil.Dugeon.NodePresets
{
    public class FinalBossNodeBehaviour : DungeonNodeBehaviour
    {
        public override void OnEnter()
        {
            Debug.Log("Final Boss Node Entered");
        }

        public override void OnExit(NodeExitInfo exitInfo)
        {
            Debug.Log("Final Boss Node Exited");
        }
    }
}