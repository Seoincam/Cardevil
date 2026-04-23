using UnityEngine;

namespace Cardevil.Gameplay.SpecialScenes
{
    public class ShopCore : SpecialSceneCore
    {
        public override string Title => "Shop";
        public override string Description => $"Floor {Context.floor} shop entrance.\nThis is the initial shell scene. Closing it clears the shop node and returns to the map.";
        public override Color AccentColor => new(0.72f, 0.54f, 0.18f, 0.96f);
    }
}
