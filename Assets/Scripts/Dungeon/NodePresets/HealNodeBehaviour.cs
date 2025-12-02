using Cardevil.Dungeon;
using Cardevil.Utils;
using UnityEngine;

namespace Cardevil.Dungeon.NodePresets
{
    public class HealNodeBehaviour : DungeonNodeBehaviour
    {
        public override void OnEnter()
        {
            LogEx.Log("Entering Heal Node: Player is healed.");
        }

        public override void OnExit(NodeExitInfo exitInfo)
        {
            LogEx.Log("Exiting Heal Node.");
        }
    }
}