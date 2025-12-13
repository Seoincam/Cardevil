using Cardevil.Cards.Data.Enhancement;
using Cardevil.Pools;
using Cardevil.Save;
using Cardevil.Sound;
using Cysharp.Threading.Tasks;
using Database;
using System.Threading;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Cardevil.Core.Bootstrap
{
    /// <summary>
    /// 게임 부트스트랩 진입점.
    /// 핵심 매니저 초기화 및 전역 접근 제공.
    /// </summary>
    public class Bootstrapper : MonoBehaviour
    {
        public static Bootstrapper Instance { get; private set; }
        
        [field: SerializeField] public GameManager Game { get; private set; }
        [field: SerializeField] public GameFlowManager GameFlow { get; private set; }
        [field: SerializeField] public SaveLoadManager SaveLoad { get; private set; }
        [field: SerializeField] public SoundManager Sound { get; private set; }
        [field: SerializeField] public PoolManager Pool { get; private set; }
        
        [field: SerializeField] public EnhancementDataLibrary CardEnhancementData { get; private set; } 

        [Header("References")]
        [SerializeField] private EventSystem eventSystem;
        [field: SerializeField] public DatabaseManager Database { get; private set; }
        
        

        private int TotalToLoad => 5;
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
            BootstrapFlow.BootstrapAsync(this, TotalToLoad, _cts.Token).Forget();
        }

        private void OnDestroy()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
        }
    }
}