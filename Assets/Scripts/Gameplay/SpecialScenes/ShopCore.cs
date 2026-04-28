using UnityEngine;

namespace Cardevil.Gameplay.SpecialScenes
{
    public class ShopCore : SpecialSceneCore
    {
        public override string TestTitle => "Shop";
        public override string TestDescription => $"Floor {Context.floor} shop entrance.\nThis is the initial shell scene. Closing it clears the shop node and returns to the map.";
        public override Color TestAccentColor => new(0.72f, 0.54f, 0.18f, 0.96f);
    }
}
