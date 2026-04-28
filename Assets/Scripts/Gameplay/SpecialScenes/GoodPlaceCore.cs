using UnityEngine;

namespace Cardevil.Gameplay.SpecialScenes
{
    public class GoodPlaceCore : SpecialSceneCore
    {
        private static readonly string[] Outcomes =
        {
            "A strange streak of luck passes through. The scene currently chooses its own outcome internally.",
            "A quiet refuge appears. Future reward or event table logic can be attached to this core.",
            "An unreadable blessing lingers here. This is the default result for entry and return flow validation.",
        };

        private string _description;

        public override void Initialize(Cardevil.Core.GameFlowManager.SpecialSceneEnterContext context)
        {
            base.Initialize(context);
            _description = Outcomes[Random.Range(0, Outcomes.Length)];
        }

        public override string TestTitle => "Good Place";
        public override string TestDescription => $"Random node result on floor {Context.floor}.\n{_description}";
        public override string TestConfirmLabel => "Finish Event";
        public override Color TestAccentColor => new(0.18f, 0.45f, 0.30f, 0.97f);
    }
}
