using Cardevil.Core.Attributes;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Cardevil.Core.SceneManagement
{
    /// <summary>
    /// 씬 참조를 위한 ScriptableObject
    /// </summary>
    [CreateAssetMenu(fileName = "SceneReference", menuName = "Scene/Scene Reference")]
    public class SceneReference : ScriptableObject
    {
        private static Dictionary<Scenes, SceneReference> sceneReferenceCache = new Dictionary<Scenes, SceneReference>();
        private static bool _initialized;
        
        public static SceneReference Find(Scenes scene)
        {
            if (sceneReferenceCache.TryGetValue(scene, out var sceneReference))
            {
                return sceneReference;
            }
            Debug.LogError($"SceneReference for {scene} not found!");
            return null;
        }
        
        public static void InitializeCache()
        {
            if (_initialized) return;
            
            sceneReferenceCache.Clear();
            var sceneReferences = Resources.LoadAll<SceneReference>("");
            foreach (var sceneReference in sceneReferences)
            {
                sceneReferenceCache.TryAdd(sceneReference.sceneEnum, sceneReference);
            }
            
            _initialized = true;
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