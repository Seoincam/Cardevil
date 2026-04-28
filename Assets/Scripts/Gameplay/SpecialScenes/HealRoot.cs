using Cardevil.Core;
using Cardevil.Core.Bootstrap;
using Cardevil.Core.SceneManagement;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Cardevil.Gameplay.SpecialScenes
{
    public class HealRoot : SpecialSceneRoot
    {
        [SerializeField] private HealView view;

        private readonly HealCore _core = new();

        protected override Scenes SceneType => Scenes.Heal;
        protected override SpecialSceneView View => view;

        protected override void Bind(GameFlowManager.SpecialSceneEnterContext context)
        {
            _core.Initialize(context);
            view.GoldChoiceSelected -= HandleGoldChoiceSelected;
            view.HealChoiceSelected -= HandleHealChoiceSelected;
            view.GoldChoiceSelected += HandleGoldChoiceSelected;
            view.HealChoiceSelected += HandleHealChoiceSelected;
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

        private void HandleGoldChoiceSelected()
        {
            _core.ApplyGoldChoice(CardevilCore.PlayerStatus);
        }

        private void HandleHealChoiceSelected()
        {
            _core.ApplyHealChoice(CardevilCore.PlayerStatus);
        }
    }
}
