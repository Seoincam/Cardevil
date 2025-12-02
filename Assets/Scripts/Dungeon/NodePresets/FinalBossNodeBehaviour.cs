using Cardevil.Dungeon;
using Cardevil.Utils;
using UnityEngine;

namespace Cardevil.Dungeon.NodePresets
{
    public class FinalBossNodeBehaviour : DungeonNodeBehaviour
    {
        public override void OnEnter()
        {
            LogEx.Log("Entering Final Boss Node: Fight the final boss.");
        }

        public override void OnExit(NodeExitInfo exitInfo)
        {
            LogEx.Log("Exiting Final Boss Node.");
        }
    }
}