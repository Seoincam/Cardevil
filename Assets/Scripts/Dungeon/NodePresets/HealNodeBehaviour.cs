using Cardevil.Dungeon;
using Cardevil.Dungeon.Core;
using Cardevil.Utils;
using UnityEngine;

namespace Cardevil.Dugeon.NodePresets
{
    public class HealNodeBehaviour : DungeonNodeBehaviour
    {
        public override void OnEnter()
        {
            LogEx.Log("Entering Heal Node: Player is healed.");
        }

        public override void OnExit(NodeClearInfo clearInfo)
        {
            LogEx.Log("Exiting Heal Node.");
        }
    }
}