using UnityEngine;

namespace Cardevil.Gameplay.SpecialScenes
{
    public class HealCore : SpecialSceneCore
    {
        public override string Title => "Heal";
        public override string Description => $"Floor {Context.floor} recovery point.\nClosing this scene clears the heal node and returns to the map.";
        public override string ConfirmLabel => "Recover";
        public override Color AccentColor => new(0.22f, 0.67f, 0.58f, 0.96f);
    }
}
