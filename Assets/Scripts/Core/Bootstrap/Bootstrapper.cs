using Cardevil.Attributes;
using Cardevil.Manager;
using Cardevil.SceneManagement;
using Cardevil.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Cardevil.Core.Bootstrap
{
    public class Bootstrapper : MonoBehaviour
    {
        /*
         * [ 초기화 해야할 것 ]
         * - DB
         * - Save Load
         */

        [Header("References")] 
        [SerializeField] private EventSystem eventSystem; 
        [SerializeField] private Slider progressBar;
        
        [Header("Loading")]
        [SerializeField, VisibleOnly] private int totalLoading;
        [SerializeField, VisibleOnly] private int loaded;
        
        [Header("Progress")]

        [SerializeField, VisibleOnly] private float progress;
        
        private float Progress
        {
            get => progress;
            set
            {
                progress = Mathf.Clamp(value, 0, 1);
                progressBar.value = progress;
            }
        }

        private int Loaded
        {
            get => loaded;
            set
            {
                loaded = value;
                Progress = (float)loaded / totalLoading;
            }
        }

        private async void Awake()
        {
            DontDestroyOnLoad(eventSystem);
            
            progressBar.value = 0f;
            
            // TODO: 씬 이름 등 따로 관리하기
            await SceneLoader.LoadSceneAsync(Scenes.Managers, LoadSceneMode.Additive);
            
            var managersScene = SceneManager.GetSceneByName("Managers");
            var managers = managersScene.GetRootGameObjects();
            totalLoading = managers.Length;

            foreach (var root in managers)
            {
                if (root.TryGetComponent<IManager>(out var manager))
                {
                    await manager.InitializeAsync();
                }
                    
                Loaded++;
            }
            LogEx.Log("All Managers loaded.");
            
            // TODO: UI/Stage 씬 로드
            await SceneLoader.LoadSceneAsync(Scenes.Title, LoadSceneMode.Additive);
            SceneLoader.SetActiveScene(Scenes.Title);
            await SceneLoader.UnloadSceneAsync(Scenes.Bootstrap);
        }
    }
}
