using Cardevil.Dungeon;
using Cardevil.Dungeon.Core;
using Cardevil.Utils;
using UnityEngine;

namespace Cardevil.Dugeon.NodePresets
{
    public class RandomNodeBehaviour : DungeonNodeBehaviour
    {
        public override void OnEnter()
        {
            LogEx.Log("Entering Random Node: Player encounters a random event.");
        }

        public override void OnExit(NodeClearInfo clearInfo)
        {
            LogEx.Log("Exiting Random Node: Player leaves the random event.");
        }
    }
}