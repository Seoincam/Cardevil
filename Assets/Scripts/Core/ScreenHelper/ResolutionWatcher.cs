using Cardevil.Core.Utils;
using UnityEngine;

namespace Cardevil.Core.ScreenHelper
{
    public class ResolutionWatcher : Singleton<ResolutionWatcher>
    {
        public Vector2Int CurrentResolution { get; private set; }
        public event System.Action<Vector2Int> OnResolutionChanged;

        private void Start()
        {
            CurrentResolution = new Vector2Int(Screen.width, Screen.height);
        }

        private void Update()
        {
            if (Screen.width != CurrentResolution.x || Screen.height != CurrentResolution.y)
            {
                CurrentResolution = new Vector2Int(Screen.width, Screen.height);
                OnResolutionChanged?.Invoke(CurrentResolution);
            }
        }
    }
}