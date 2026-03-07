using System.Collections.Generic;

namespace Cardevil.Card.InStage.Score.Step
{
    public static class ScoreVisualRegistry
    {
        private static readonly Dictionary<IScoreSource, IScoreVisual> Registry = new();

        public static void Register(IScoreSource sourceModel, IScoreVisual visualRegistryTarget)
        {
            if (sourceModel == null || visualRegistryTarget == null) return;

            Registry[sourceModel] = visualRegistryTarget;
        }

        public static void Unregister(IScoreSource sourceModel)
        {
            if (sourceModel == null) return;
            
            Registry.Remove(sourceModel);
        }

        public static bool TryGet(IScoreSource sourceModel, out IScoreVisual visualRegistryTarget)
        {
            if (sourceModel == null)
            {
                visualRegistryTarget = null;
                return false;
            }
            
            return Registry.TryGetValue(sourceModel, out visualRegistryTarget);
        }

        public static void Clear()
        {
            Registry.Clear();
        }
    }
}