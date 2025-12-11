using Cardevil.SceneManagement;
using Cardevil.Utils;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Cardevil.Title
{
    public class TitleManager : MonoBehaviour
    {
        [Header("UI")] 
        [SerializeField] private Button startButton;
        [SerializeField] private Button codexButton;
        [SerializeField] private Button settingButton;
        [SerializeField] private Button creditButton;
        [SerializeField] private Button exitButton;

        private void Awake()
        {
            BindButtons();
        }

        private void BindButtons()
        {
            startButton.onClick.AddListener(OnStartButtonClicked);
            codexButton.onClick.AddListener(OnCodexButtonClicked);
            settingButton.onClick.AddListener(OnSettingsButtonClicked);
            creditButton.onClick.AddListener(OnCreditButtonClicked);
            exitButton.onClick.AddListener(OnExitButtonClicked);
        }

        private void OnStartButtonClicked()
        {
            async UniTask EnterGameAsync()
            {
                await SceneLoader.LoadSceneAsync(Scenes.GamePlay, LoadSceneMode.Single);
                SceneLoader.SetActiveScene(Scenes.GamePlay);
            }
            
            EnterGameAsync().Forget();
        }

        private void OnCodexButtonClicked()
        {
            LogEx.Log("도감. 아직 미구현.");   
        }

        private void OnSettingsButtonClicked()
        {
            LogEx.Log("설정. 아직 미구현.");
        }

        private void OnCreditButtonClicked()
        {
            LogEx.Log("제작진. 아직 미구현.");
        }

        private void OnExitButtonClicked()
        {
            Application.Quit();
        }
    }
}
