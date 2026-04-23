using UnityEngine;

namespace Cardevil.Gameplay.SpecialScenes
{
    public class BlackMarketCore : SpecialSceneCore
    {
        public override string Title => "Black Market";
        public override string Description => $"Floor {Context.floor}의 블랙 마켓입니다.\n임시 UI입니다. 닫으면 블랙 마켓 노드가 제거됩니다.";
        public override string ConfirmLabel => "떠나기";
        public override Color AccentColor => new(0.34f, 0.18f, 0.18f, 0.97f);
    }
}
