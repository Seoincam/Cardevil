using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Cardevil.SceneManagement
{
    public static class SceneLoader
    {
        public static async UniTask LoadSceneAsync(Scenes scene, LoadSceneMode mode)
        {
            var reference = SceneReference.Find(scene);
            await SceneManager.LoadSceneAsync(reference, mode);
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