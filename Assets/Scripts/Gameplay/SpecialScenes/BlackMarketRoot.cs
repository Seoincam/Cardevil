using Cardevil.Core;
using Cardevil.Core.SceneManagement;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Cardevil.Gameplay.SpecialScenes
{
    public class BlackMarketRoot : SpecialSceneRoot
    {
        [SerializeField] private BlackMarketView view;

        private readonly BlackMarketCore _core = new();

        protected override Scenes SceneType => Scenes.BlackMarket;
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
