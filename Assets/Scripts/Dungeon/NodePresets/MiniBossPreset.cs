using Cardevil.Dungeon;
using UnityEngine;

namespace Cardevil.Dugeon.NodePresets
{
    public class MiniBossPreset : DungeonNodePreset
    {
        public override void OnEnter()
        {
            Debug.Log("Mini Boss Node Entered");
        }

        public override void OnExit()
        {
            Debug.Log("Mini Boss Node Exited");
        }
    }
}