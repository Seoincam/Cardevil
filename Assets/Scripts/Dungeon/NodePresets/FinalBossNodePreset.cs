using Cardevil.Dungeon;
using UnityEngine;

namespace Cardevil.Dugeon.NodePresets
{
    public class FinalBossNodePreset : DungeonNodePreset
    {
        public override void OnEnter()
        {
            Debug.Log("Final Boss Node Entered");
        }

        public override void OnExit()
        {
            Debug.Log("Final Boss Node Exited");
        }
    }
}