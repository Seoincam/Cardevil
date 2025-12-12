using Cardevil.Save;
using Cardevil.Sound;
using Cysharp.Threading.Tasks;
using Database;
using System.Threading;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Cardevil.Core.Bootstrap
{
    public class Bootstrapper : MonoBehaviour
    {
        public static Bootstrapper Instance { get; private set; }
        
        [field: SerializeField] public GameManager Game { get; private set; }
        [field: SerializeField] public GameFlowManager GameFlow { get; private set; }
        [field: SerializeField] public SaveLoadManager SaveLoad { get; private set; }
        [field: SerializeField] public SoundManager Sound { get; private set; }

        [Header("References")]
        [SerializeField] private EventSystem eventSystem;
        [field: SerializeField] public DatabaseManager Database { get; private set; }
        

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
            BootstrapFlow.RunAsync(this, 4, _cts.Token).Forget();
        }

        private void OnDestroy()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
        }
    }
}