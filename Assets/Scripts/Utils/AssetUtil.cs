using Cardevil.Core.Bootstrap;
using Cardevil.Pools;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Cardevil.Utils
{
    public static class AssetUtil
    {
        public static T Load<T>(string path) where T : Object
        {
            if (typeof(T) == typeof(GameObject))
            {
                string name = path;
                int index = name.LastIndexOf('/');
                if (index >= 0)
                {
                    name = name.Substring(index + 1);
                }
                if (CardevilCore.Instance.Pool.TryGetOriginal(name, out Poolable original))
                {
                    return original.gameObject as T;
                }
            }

        
            T resource = Resources.Load<T>(path);
            if (resource == null)
            {
                LogEx.LogWarning($"Resource not found in Resources folder: {path}. Trying Addressables...");
            }
            else
            {
                return resource;
            }
        
            var op = Addressables.LoadAssetAsync<T>(path);
            op.WaitForCompletion();
            if (op.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
            {
                return op.Result;
            }

            return resource;
        }
        
        public static List<T> LoadAll<T>(string path) where T : Object
        {
            List<T> resources = new List<T>(Resources.LoadAll<T>(path));
            
            try
            {
                var op = Addressables.LoadAssetsAsync<T>(path, null);
                op.WaitForCompletion();
                if (op.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
                {
                    resources.AddRange(op.Result);
                }
            }
            catch (System.Exception e)
            {
                LogEx.LogWarning($"Failed to load assets from Addressables with path: {path}. Exception: {e}");
            }
            
            return resources;
        }

        public static GameObject Instantiate(string path, Transform parent = null)
        {
            GameObject original = Load<GameObject>($"Prefabs/{path}");
            if (original == null)
            {
                Debug.Log($"Failed to load prefab : {path}");
                return null;
            }

            if (original.TryGetComponent<Poolable>(out Poolable poolable))
            {
                return CardevilCore.Instance.Pool.GetFromOriginal(poolable, parent).gameObject;
            }

            GameObject go = Object.Instantiate(original, parent);
            go.name = original.name;

            return go;
        }

        public static GameObject Instantiate(string path, Vector3 position, Transform parent = null)
        {
            GameObject prefab = Load<GameObject>($"Prefabs/{path}");
            if (prefab == null)
            {
                Debug.Log($"Failed to load prefab : {path}");
                return null;
            }
            return Object.Instantiate(prefab, position, Quaternion.identity, parent);
        }

        public static void Destroy(GameObject go)
        {
            if (go == null)
            {
                return;
            }
            Poolable poolable = go.GetComponent<Poolable>();
            if (poolable != null)
            {
                poolable.Release();
                return;
            }
            Object.Destroy(go);
        }
    }
}
