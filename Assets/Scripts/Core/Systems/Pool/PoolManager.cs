using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Pool;
using Object = UnityEngine.Object;

namespace Cardevil.Core.Systems.Pool
{
    [Serializable]
    public class PoolRegistration
    {
        [SerializeField] private Poolables _enumKey = Poolables.None;
        [SerializeField] private string _stringKey;
        [SerializeField] private GameObject _prefab;

        public Poolables EnumKey => _enumKey;
        public string StringKey => string.IsNullOrWhiteSpace(_stringKey) ? _prefab != null ? _prefab.name : string.Empty : _stringKey;
        public GameObject Prefab => _prefab;
    }

    [Serializable]
    public class PoolManager : IClearable
    {
        [SerializeField] private int _initialSize = 3;
        [SerializeField] private int _maxSize = 100;
        [SerializeField] private List<PoolRegistration> _registrations = new();

        private readonly Dictionary<string, Poolable> _originals = new(StringComparer.Ordinal);
        private readonly Dictionary<string, IObjectPool<Poolable>> _pools = new(StringComparer.Ordinal);
        private readonly Dictionary<Type, IObjectPool<Poolable>> _typePools = new();
        private readonly Dictionary<Poolables, string> _enumKeys = new();
        private readonly Dictionary<Poolable, string> _originalKeys = new();

        private Transform _rootTransform;
        private Transform _tempPoolParent;

        public void Init(Transform parent)
        {
            if (_rootTransform == null)
            {
                _rootTransform = new GameObject("@Poolable_Root").transform;
                _rootTransform.SetParent(parent, false);
            }

            foreach (PoolRegistration registration in _registrations)
            {
                RegisterRegistration(registration);
            }
        }

        public void Clear()
        {
            foreach (var pool in _pools.Values)
            {
                pool.Clear();
            }

            _pools.Clear();
            _typePools.Clear();
            _originals.Clear();
            _enumKeys.Clear();
            _originalKeys.Clear();
        }

        public void RegisterPrefab(GameObject prefab)
        {
            RegisterPrefab(prefab, prefab != null ? prefab.name : string.Empty, Poolables.None);
        }

        public void RegisterPrefab(GameObject prefab, string key)
        {
            RegisterPrefab(prefab, key, Poolables.None);
        }

        public void RegisterPrefab(GameObject prefab, string key, Poolables enumKey)
        {
            if (prefab == null)
            {
                Debug.LogError("Prefab cannot be null.");
                return;
            }

            if (!prefab.TryGetComponent(out Poolable original))
            {
                Debug.LogWarning($"Prefab {prefab.name} does not contain a Poolable component.");
                return;
            }

            RegisterOriginal(original, key, enumKey);
        }

        public void RegisterOriginal(Poolable original)
        {
            RegisterOriginal(original, original != null ? original.name : string.Empty, Poolables.None);
        }

        public void RegisterOriginal(Poolable original, string key)
        {
            RegisterOriginal(original, key, Poolables.None);
        }

        public void RegisterOriginal(Poolable original, string key, Poolables enumKey)
        {
            if (original == null)
            {
                Debug.LogError("Poolable cannot be null.");
                return;
            }

            if (string.IsNullOrWhiteSpace(key))
            {
                Debug.LogError("Pool key cannot be null or empty.");
                return;
            }

            if (_originals.ContainsKey(key))
            {
                Debug.LogWarning($"Pool for key {key} already exists. Overwriting.");
            }

            _originals[key] = original;
            _originalKeys[original] = key;
            _pools[key] = new ObjectPool<Poolable>(
                () => Object.Instantiate(original),
                ActionOnGet,
                ActionOnRelease,
                null,
                true,
                _initialSize,
                _maxSize
            );

            if (enumKey != Poolables.None)
            {
                _enumKeys[enumKey] = key;
            }

            RegisterTypePools(original, _pools[key], key);
        }

        [Obsolete("Use RegisterOriginal instead.")]
        public void RegisterFactory(Poolable poolable)
        {
            RegisterOriginal(poolable);
        }

        [Obsolete("Use RegisterOriginal instead.")]
        public void RegisterFactory(Poolable poolable, string type)
        {
            RegisterOriginal(poolable, type);
        }

        private void RegisterRegistration(PoolRegistration registration)
        {
            if (registration == null || registration.Prefab == null)
            {
                return;
            }

            RegisterPrefab(registration.Prefab, registration.StringKey, registration.EnumKey);
        }

        private void RegisterTypePools(Poolable original, IObjectPool<Poolable> pool, string key)
        {
            MonoBehaviour[] behaviours = original.GetComponents<MonoBehaviour>();
            foreach (MonoBehaviour behaviour in behaviours)
            {
                if (behaviour is not IPoolableSubComponent)
                {
                    continue;
                }

                Type componentType = behaviour.GetType();
                if (_typePools.ContainsKey(componentType))
                {
                    Debug.LogWarning($"Type pool for {componentType.Name} already exists. Overwriting with {key}.");
                }

                _typePools[componentType] = pool;
            }
        }

        private void ActionOnGet(Poolable poolable)
        {
            poolable.transform.SetParent(_tempPoolParent, false);
            poolable.OnGetFromPool();
        }

        private void ActionOnRelease(Poolable poolable)
        {
            poolable.transform.SetParent(_rootTransform, false);
            poolable.OnReturnToPool();
        }

        public void Release(Poolable poolable)
        {
            if (poolable == null)
            {
                return;
            }

            if (poolable._pool != null)
            {
                poolable._pool.Release(poolable);
                return;
            }

            Debug.LogWarning($"Poolable {poolable.name} is not owned by any pool. Destroying.");
            Object.Destroy(poolable.gameObject);
        }

        public Poolable GetFromOriginal(Poolable original, Transform parent = null)
        {
            if (original == null)
            {
                Debug.LogError("Poolable cannot be null.");
                return null;
            }

            if (!_originalKeys.TryGetValue(original, out string key))
            {
                RegisterOriginal(original);
                key = original.name;
            }

            return Get(key, parent);
        }

        public Poolable Get(Poolables type, Transform parent = null)
        {
            if (_enumKeys.TryGetValue(type, out string key))
            {
                return Get(key, parent);
            }

            return Get(type.ToString(), parent);
        }

        public Poolable Get(string type, Transform parent = null)
        {
            if (string.IsNullOrWhiteSpace(type))
            {
                Debug.LogError("Pool key cannot be null or empty.");
                return null;
            }

            if (!EnsureRegistered(type))
            {
                Debug.LogError($"No pool registered for key {type}.");
                return null;
            }

            _tempPoolParent = parent;
            IObjectPool<Poolable> pool = _pools[type];
            Poolable poolable = pool.Get();
            poolable._pool = pool;
            return poolable;
        }

        public Poolable Get(GameObject prefab, Transform parent = null)
        {
            if (prefab == null)
            {
                Debug.LogError("Prefab cannot be null.");
                return null;
            }

            if (!prefab.TryGetComponent(out Poolable original))
            {
                Debug.LogError($"Prefab {prefab.name} does not contain a Poolable component.");
                return null;
            }

            return GetFromOriginal(original, parent);
        }

        public T Get<T>(Poolables type, Transform parent = null) where T : MonoBehaviour
        {
            return Get<T>(type.ToString(), parent);
        }

        public T Get<T>(string type, Transform parent = null) where T : MonoBehaviour
        {
            Poolable poolable = Get(type, parent);
            return ExtractComponent<T>(poolable, type);
        }

        public T Get<T>(GameObject prefab, Transform parent = null) where T : MonoBehaviour
        {
            Poolable poolable = Get(prefab, parent);
            return ExtractComponent<T>(poolable, prefab != null ? prefab.name : typeof(T).Name);
        }

        public T Get<T>(Transform parent = null) where T : MonoBehaviour, IPoolableSubComponent
        {
            if (!_typePools.TryGetValue(typeof(T), out IObjectPool<Poolable> pool))
            {
                EnsureRegistered(typeof(T).Name);
            }

            if (!_typePools.TryGetValue(typeof(T), out pool))
            {
                Debug.LogError($"No type pool registered for {typeof(T).Name}.");
                return null;
            }

            _tempPoolParent = parent;
            Poolable poolable = pool.Get();
            poolable._pool = pool;
            return poolable.GetComponent<T>();
        }

        public Poolable GetOriginal(Poolables type)
        {
            if (_enumKeys.TryGetValue(type, out string key))
            {
                return GetOriginal(key);
            }

            return GetOriginal(type.ToString());
        }

        public Poolable GetOriginal(string type)
        {
            if (TryGetOriginal(type, out Poolable original))
            {
                return original;
            }

            Debug.LogError($"No original registered for key {type}.");
            return null;
        }

        public bool TryGetOriginal(string type, out Poolable original)
        {
            if (string.IsNullOrWhiteSpace(type))
            {
                original = null;
                return false;
            }

            EnsureRegistered(type);
            return _originals.TryGetValue(type, out original);
        }

        private bool EnsureRegistered(string key)
        {
            if (_pools.ContainsKey(key))
            {
                return true;
            }

            GameObject prefab = TryLoadPrefab(key);
            if (prefab == null)
            {
                return false;
            }

            if (!prefab.TryGetComponent(out Poolable poolable))
            {
                return false;
            }

            RegisterOriginal(poolable, key);
            return _pools.ContainsKey(key);
        }

        private static T ExtractComponent<T>(Poolable poolable, string type) where T : MonoBehaviour
        {
            if (poolable == null)
            {
                return null;
            }

            if (poolable.TryGetComponent(out T component))
            {
                return component;
            }

            if (poolable is T directComponent)
            {
                return directComponent;
            }

            Debug.LogError($"Requested type {typeof(T)} does not match the poolable from pool {type}.");
            return null;
        }

        private static GameObject TryLoadPrefab(string key)
        {
            GameObject prefab = Resources.Load<GameObject>($"Prefabs/{key}");
            if (prefab != null)
            {
                return prefab;
            }

            // 어드레서블사용하는 것으로 언젠가는 바꿀예정...
            //
            // try
            // {
            //     var op = Addressables.LoadAssetAsync<GameObject>($"Prefabs/{key}");
            //     op.WaitForCompletion();
            //     if (op.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
            //     {
            //         return op.Result;
            //     }
            // }
            // catch (Exception)
            // {
            //     // Pool lookup should fail quietly for non-pooled prefabs.
            // }

            return null;
        }
    }
}
