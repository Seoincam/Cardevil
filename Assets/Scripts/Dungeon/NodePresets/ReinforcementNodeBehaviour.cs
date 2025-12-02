using Cardevil.Dungeon;
using Cardevil.Utils;
using UnityEngine;

namespace Cardevil.Dungeon.NodePresets
{
    public class ReinforcementNodeBehaviour : DungeonNodeBehaviour
    {
        public override void OnEnter()
        {
            LogEx.Log("Reinforcement Node Entered");
        }

        public override void OnExit(NodeExitInfo exitInfo)
        {
            LogEx.Log("Reinforcement Node Exited");
        }
    }
}