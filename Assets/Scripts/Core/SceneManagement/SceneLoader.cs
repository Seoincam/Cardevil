using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Cardevil.SceneManagement
{
    public static class SceneLoader
    {
        public static event Action<Scenes, LoadSceneMode> SceneLoaded;
        
        /// <summary>
        /// 씬 로드 끝까지 대기 후 <see cref="SceneLoaded"/> 발행.
        /// </summary>
        public static async UniTask LoadSceneAsync(Scenes scene, LoadSceneMode mode)
        {
            var op = LoadSceneHandle(scene, mode, true);
            await op;
        }

        /// <summary>
        /// AsyncOperation 반한. 호출자가 직접 제어.
        /// </summary>
        /// <param name="activateOnLoad"><c>true</c>일 시, <c>completed</c>에서 <see cref="SceneLoaded"/> 발행.</param>
        public static AsyncOperation LoadSceneHandle(Scenes scene, LoadSceneMode mode, bool activateOnLoad = true)
        {
            var reference = SceneReference.Find(scene);
            var op = SceneManager.LoadSceneAsync(reference, mode);
            op.allowSceneActivation = activateOnLoad;

            if (activateOnLoad)
                op.completed += _ => SceneLoaded?.Invoke(scene, mode);

            return op;
        }
        
        public static async UniTask UnloadSceneAsync(Scenes scene)
        {
            var reference = SceneReference.Find(scene);
            await SceneManager.UnloadSceneAsync(reference);
        }

        public static void SetActiveScene(Scenes scene)
        {
            var reference = SceneReference.Find(scene);
            var s = SceneManager.GetSceneByName(reference);
            SceneManager.SetActiveScene(s);
        }

        /// <summary>
        /// 외부에서 <c>allowSceneActivation</c>을 <c>true</c>로 켠 후,
        /// 씬 활성화 완료까지 기다렸다가 <see cref="SceneLoaded"/> 발행.
        /// </summary>
        public static async UniTask WaitSceneActivationAsync(Scenes scene, AsyncOperation op, LoadSceneMode mode,
            CancellationToken ct)
        {
            await op.ToUniTask(cancellationToken: ct);
            SceneLoaded?.Invoke(scene, mode);
        }
    }
}