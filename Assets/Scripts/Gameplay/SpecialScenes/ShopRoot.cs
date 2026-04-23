using Cardevil.Core;
using Cardevil.Core.SceneManagement;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Cardevil.Gameplay.SpecialScenes
{
    public class ShopRoot : SpecialSceneRoot
    {
        [SerializeField] private ShopView view;

        private readonly ShopCore _core = new();

        protected override Scenes SceneType => Scenes.Shop;
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
