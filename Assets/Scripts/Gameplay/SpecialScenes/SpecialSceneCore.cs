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

        public abstract string Title { get; }
        public abstract string Description { get; }
        public virtual string ConfirmLabel => "확인";
        public abstract Color AccentColor { get; }
    }
}
