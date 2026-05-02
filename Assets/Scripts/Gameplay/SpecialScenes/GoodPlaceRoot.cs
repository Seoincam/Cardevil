using Cardevil.Core;
using Cardevil.Core.SceneManagement;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Cardevil.Gameplay.SpecialScenes
{
    public class GoodPlaceRoot : SpecialSceneRoot
    {
        [SerializeField] private GoodPlaceView view;

        private readonly GoodPlaceCore _core = new();

        protected override Scenes SceneType => Scenes.GoodPlace;
        protected override SpecialSceneView View => view;

        protected override void Bind(GameFlowManager.SpecialSceneEnterContext context)
        {
            _core.Initialize(context);
            view.Bind(_core);
        }

        protected override UniTask PlayEnterAsync()
        {
            return view.PlayEnterAsync();
        }

        protected override UniTask PlayExitAsync()
        {
            return view.PlayExitAsync();
        }
    }
}
