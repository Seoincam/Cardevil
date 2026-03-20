using Cardevil.Core.Systems.Pool;
using Cardevil.Core.Systems.Save;
using Cardevil.Core.Systems.Sounds;
using Cardevil.Gameplay;
using Cysharp.Threading.Tasks;
using Database;
using System.Threading;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Cardevil.Core.Bootstrap
{
    /// <summary>
    /// 핵심 매니저 전역 접근 제공.
    /// </summary>
    public class CardevilCore : MonoBehaviour
    {
        public static CardevilCore Instance { get; private set; }
        
        [Header("Settings")] 
        [field: SerializeField] public bool CanSelectSaveSlot { get; private set; }
        
        [field: Header("Managers")]
        [field: SerializeField] public GameManager GameManager { get; private set; }
        [field: SerializeField] public GameFlowManager GameFlowManager { get; private set; }
        [field: SerializeField] public SaveLoadManager SaveLoadManager { get; private set; }
        [field: SerializeField] public SoundManager SoundManager { get; private set; }
        [field: SerializeField] public PoolManager PoolManager { get; private set; }
        [field: SerializeField] public DatabaseManager DatabaseManager { get; private set; }
        
        // 외부 단축 접근용
        public static GameManager Game => Instance.GameManager;
        public static GameFlowManager GameFlow => Instance.GameFlowManager;
        public static SaveLoadManager SaveLoad => Instance.SaveLoadManager;
        public static SoundManager Sound => Instance.SoundManager;
        public static PoolManager Pool => Instance.PoolManager;
        public static DatabaseManager Database => Instance.DatabaseManager;
        
        // 자주 쓰는 깊은 계층 단축
        public static PlayerStatus PlayerStatus => Instance.GameManager.PlayerStatus;
        
        [Header("References")]
        [SerializeField] private EventSystem eventSystem;
        
        private int TotalToLoad => 6;
        private CancellationTokenSource _cts;

        private void Awake()
        {
            if (Instance)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            DontDestroyOnLoad(eventSystem);
            
            _cts = new CancellationTokenSource();
            Bootstrapper.BootstrapAsync(this, TotalToLoad, _cts.Token).Forget();
        }

        private void OnDestroy()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
        }
    }
}