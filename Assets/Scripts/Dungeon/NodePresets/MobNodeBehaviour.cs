using Cardevil.Dungeon;
using Cardevil.Dungeon.Core;
using Cardevil.Utils;
using UnityEngine;

namespace Cardevil.Dugeon.NodePresets
{
    public class MobNodeBehaviour : DungeonNodeBehaviour
    {
        public override void OnEnter()
        {
            LogEx.Log("Entering Mob Node");
        }

        public override void OnExit(NodeClearInfo clearInfo)
        {
            LogEx.Log("Exiting Mob Node");
        }
    }
}