using Cardevil.Pools;
using Cardevil.Card.InStage.Score.Step;
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
    /// 핵심 매니저 전역 접근 제공.
    /// </summary>
    public class CardevilCore : MonoBehaviour
    {
        public static CardevilCore Instance { get; private set; }
        
        [Header("Settings")] 
        [field: SerializeField] public bool CanSelectSaveSlot { get; private set; }
        
        [SerializeField] private GameManager game;
        [SerializeField] private GameFlowManager gameFlow;
        [SerializeField] private SaveLoadManager saveLoad;
        [SerializeField] private SoundManager sound;
        [SerializeField] private PoolManager pool;
        [SerializeField] private DatabaseManager database;

        // Bootstrapper 등 내부 로직용
        public GameManager GameManager => game;
        public GameFlowManager GameFlowManager => gameFlow;
        public SaveLoadManager SaveLoadManager => saveLoad;
        public SoundManager SoundManager => sound;
        public PoolManager PoolManager => pool;
        public DatabaseManager DatabaseManager => database;

        // 외부 단축 접근용
        public static GameManager Game => Instance.game;
        public static GameFlowManager GameFlow => Instance.gameFlow;
        public static SaveLoadManager SaveLoad => Instance.saveLoad;
        public static SoundManager Sound => Instance.sound;
        public static PoolManager Pool => Instance.pool;
        public static DatabaseManager Database => Instance.database;
        
        // 자주 쓰는 깊은 계층 단축
        public static ScoreProviderRegistry Score => Instance.game.ScoreProviderRegistry;
        
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