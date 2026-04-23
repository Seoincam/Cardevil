using Cardevil.Core.Utils;
using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Cardevil.Core.SceneManagement
{
    public static class SceneLoader
    {
        /// <summary>
        /// SceneLoader를 통해 씬이 로드된 후에 발행. (Additive, Single 모두 발행)
        /// </summary>
        public static event Action<Scenes, LoadSceneMode> SceneLoaded;
        
        /// <summary>
        /// SceneLoader를 통해 씬이 언로드된 후에 발행. <br/>
        /// <b>주의:</b> LoadSceneMode.Single로 씬이 로드될 때는 이전 씬이 날아가지만 이 이벤트는 발행되지 않음.
        /// </summary>
        public static event Action<Scenes> SceneManuallyUnloaded;
        
        /// <summary>
        /// 실제 Active 씬이 변경되었을 때만 발행. (이전 씬, 새 씬)
        /// </summary>
        public static event Action<Scenes, Scenes> ActiveSceneChanged;
        
        /// <summary>
        /// 씬 로드 끝까지 대기.
        /// </summary>
        public static async UniTask LoadSceneAsync(Scenes scene, LoadSceneMode mode)
        {
            var op = LoadSceneHandle(scene, mode, true);
            await op;
        }

        /// <summary>
        /// AsyncOperation 반환.
        /// allowSceneActivation이 false일경우 op는 IsDone이 true가 되지 않음.
        /// </summary>
        /// <param name="scene"></param>
        /// <param name="mode"></param>
        /// <param name="activateOnLoad">
        /// true이면 로드 완료 시 Unity의 씬 활성화 단계를 바로 진행한다.
        /// false이면 progress 0.9f에서 멈추며, 호출자가 op.allowSceneActivation = true로 완료 시점을 제어해야함
        /// </param>
        /// <returns></returns>
        public static AsyncOperation LoadSceneHandle(Scenes scene, LoadSceneMode mode, bool activateOnLoad = true)
        {
            var reference = SceneReference.Find(scene);
            var op = SceneManager.LoadSceneAsync(reference, mode);
            op.allowSceneActivation = activateOnLoad;
            
            Scenes prevActiveScene = SceneReference.Find(SceneManager.GetActiveScene().name).SceneEnum;
            
            
            op.completed += _ =>
            {
                if (op.isDone)
                {
                    SceneLoaded?.Invoke(scene, mode);
                    
                    if (mode == LoadSceneMode.Single)
                    {
                        ActiveSceneChanged?.Invoke(prevActiveScene, scene);
                    }
                }
                else
                {
                    Debug.LogError($"SceneLoader: Failed to load scene {scene}.");
                }
            };
            
            return op;
        }
            
        public static async UniTask UnloadSceneAsync(Scenes scene)
        {
            var reference = SceneReference.Find(scene);
            await SceneManager.UnloadSceneAsync(reference);
            SceneManuallyUnloaded?.Invoke(scene);
        }

        public static void SetActiveScene(Scenes scene, bool force = false)
        {
            var reference = SceneReference.Find(scene);
            var targetScene = SceneManager.GetSceneByName(reference);
            var currentActiveScene = SceneManager.GetActiveScene();
            
            if (!targetScene.IsValid() || !targetScene.isLoaded)
            {
                LogEx.LogError($"Cannot set active scene to '{scene}' because it is not loaded or does not exist.");
                return;
            }
            
            if (!force && currentActiveScene.name == targetScene.name)
            {
                return;
            }

            Scenes prevActiveSceneEnum = SceneReference.Find(currentActiveScene.name).SceneEnum;
            
            if (SceneManager.SetActiveScene(targetScene))
            {
                ActiveSceneChanged?.Invoke(prevActiveSceneEnum, scene);
            }
            else
            {
                LogEx.LogError($"Failed to set active scene to '{scene}'.");
            }
        }
    }
}