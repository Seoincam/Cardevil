
using Cardevil.Attributes;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Cardevil.SceneManagement
{
    /// <summary>
    /// 씬 참조를 위한 ScriptableObject
    /// </summary>
    [CreateAssetMenu(fileName = "SceneReference", menuName = "Scene/Scene Reference")]
    public class SceneReference : ScriptableObject
    {
        private static Dictionary<Scenes, SceneReference> sceneReferenceCache = new Dictionary<Scenes, SceneReference>();
        
        public static SceneReference Find(Scenes scene)
        {
            if (sceneReferenceCache.TryGetValue(scene, out var sceneReference))
            {
                return sceneReference;
            }
            Debug.LogError($"SceneReference for {scene} not found!");
            return null;
        }
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InitializeCache()
        {
            sceneReferenceCache.Clear();
            var sceneReferences = Resources.LoadAll<SceneReference>("");
            foreach (var sceneReference in sceneReferences)
            {
                sceneReferenceCache.TryAdd(sceneReference.sceneEnum, sceneReference);
            }
        }
        
        #if UNITY_EDITOR
        [SerializeField] private SceneAsset sceneAsset;
        #endif
        [SerializeField] private Scenes sceneEnum;
        [SerializeField, VisibleOnly] private string sceneName;
        
        public string SceneName => sceneName;
        
        
        #if UNITY_EDITOR
        private void OnValidate()
        {
            if (sceneAsset != null)
            {
                sceneName = sceneAsset.name;
            }
        }
        #endif
        
        public static implicit operator string(SceneReference sceneReference)
        {
            return sceneReference.sceneName;
        }
    }
}