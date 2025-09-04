using Cardevil.Dungeon;
using UnityEngine;

namespace Cardevil.Dugeon.NodePresets
{
    public class MobNodePreset : DungeonNodePreset
    {
        public override void OnEnter()
        {
            Debug.Log("Entering Mob Node");
        }

        public override void OnExit()
        {
            Debug.Log("Exiting Mob Node");
        }
    }
}