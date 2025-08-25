using Cardevil.Dungeon;
using UnityEngine;

namespace Cardevil.Dugeon.NodePresets
{
    public class RandomNodePreset : DungeonNodePreset
    {
        public override void OnEnter()
        {
            // Logic for entering a random node
            Debug.Log("Entering Random Node: Player encounters a random event.");
        }

        public override void OnExit()
        {
            // Logic for exiting a random node, if any
            Debug.Log("Exiting Random Node: Player leaves the random event.");
        }
    }
}