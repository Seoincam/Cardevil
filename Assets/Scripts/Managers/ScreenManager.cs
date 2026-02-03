using System;
using UnityEngine;

namespace Cardevil.Manager
{
    
    /// <summary>
    /// 화면 해상도 변화를 감지하고 알리는 매니저.
    /// </summary>
    public sealed class ScreenManager : MonoBehaviour
    {
        public static event Action<int, int> OnScreenResolutionChanged;

        // 싱글톤 인스턴스. 일단 상속은 안함
        private static ScreenManager _instance;
        public static ScreenManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindAnyObjectByType<ScreenManager>();
                    if (_instance == null)
                    {
                        GameObject obj = new GameObject("@ScreenManager");
                        _instance = obj.AddComponent<ScreenManager>();
                        DontDestroyOnLoad(obj);
                    }
                }
                return _instance;
            }
        }

        private Vector2Int lastScreenResolution;

        private void Awake()
        {
            int width = Screen.width;
            int height = Screen.height; 
            lastScreenResolution = new Vector2Int(width, height);
        }

        private void Start()
        {
            OnScreenResolutionChanged?.Invoke(lastScreenResolution.x, lastScreenResolution.y);
        }

        private void Update()
        {
            if (Screen.width != lastScreenResolution.x || Screen.height != lastScreenResolution.y)
            {
                lastScreenResolution = new Vector2Int(Screen.width, Screen.height);
                OnScreenResolutionChanged?.Invoke(Screen.width, Screen.height);
            }
        }
    }
}