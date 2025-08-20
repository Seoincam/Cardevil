using Cardevil.Dungeon;
using UnityEngine;

namespace Cardevil.Dugeon.NodePresets
{
    public class ReinforcementNodePreset : DungeonNodePreset
    {
        public override void OnEnter()
        {
            // Reinforcement logic goes here
            // For example, spawn additional enemies or enhance existing ones
            Debug.Log("Reinforcement Node Entered");
        }

        public override void OnExit()
        {
            // Cleanup or reset logic when exiting the reinforcement node
            Debug.Log("Reinforcement Node Exited");
        }
    }
}