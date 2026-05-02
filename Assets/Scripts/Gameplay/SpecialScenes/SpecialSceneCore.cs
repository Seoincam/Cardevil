using Cardevil.Core;
using UnityEngine;

namespace Cardevil.Gameplay.SpecialScenes
{
    public abstract class SpecialSceneCore
    {
        public GameFlowManager.SpecialSceneEnterContext Context { get; private set; }

        public virtual void Initialize(GameFlowManager.SpecialSceneEnterContext context)
        {
            Context = context;
        }

        public abstract string TestTitle { get; }
        public abstract string TestDescription { get; }
        public virtual string TestConfirmLabel => "확인";
        public abstract Color TestAccentColor { get; }
    }
}
