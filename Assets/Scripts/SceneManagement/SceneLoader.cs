using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Cardevil.SceneManagement
{
    public static class SceneLoader
    {
        public static event Action<Scenes, LoadSceneMode> SceneLoaded;
        
        public static async UniTask LoadSceneAsync(Scenes scene, LoadSceneMode mode)
        {
            var reference = SceneReference.Find(scene);
            await SceneManager.LoadSceneAsync(reference, mode);
            SceneLoaded?.Invoke(scene, mode);
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
    }
}