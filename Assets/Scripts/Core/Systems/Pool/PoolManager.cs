using Cardevil.Core.DataStructure.Serializables;
using Cardevil.Core.Utils;
using System;
using UnityEngine;
using UnityEngine.Pool;
using Object = UnityEngine.Object;

namespace Cardevil.Core.Systems.Pool
{
    [Serializable]
    public class PoolManager : IClearable
    {
        
        [SerializeField] private int _initialSize = 3;
        [SerializeField] private int _maxSize = 100;
        

        
        [SerializeField] private SerializableDictionary<string, ICloneFactory<Poolable>> _factories = new();
        [SerializeField] private SerializableDictionary<string, IObjectPool<Poolable>> _pools = new();
        [SerializeField] private SerializableDictionary<Type, IObjectPool<Poolable>> _typePools = new();
        /// <summary>
        /// Root Transform임
        /// 필요시 각 Poolable의 부모로 변경 가능.
        /// </summary>
        private Transform _RootTransform;
        
        private Transform _tempPoolParent;

        /// <summary>
        /// 초기화 메서드
        /// </summary>
        /// <remarks>
        /// 1. RootTransform이 null일 경우, 새로운 GameObject를 생성하여 RootTransform으로 설정
        /// 2. 등록된 모든 팩토리에 대해 ObjectPool을 생성
        /// </remarks>
        public void Init(Transform parent)
        {
            if (_RootTransform == null)
            {
                _RootTransform = new GameObject("@Poolable_Root").transform;
                _RootTransform.SetParent(parent);
            }
            
            PoolableFactoryContainerSO container = AssetUtil.Load<PoolableFactoryContainerSO>("ScriptableObjects/PoolableFactoryContainer");
            if (container == null)
            {
                Debug.LogError("PoolableFactoryContainerSO not found. Cannot initialize pools.");
            }
            else
            {
                foreach (var factory in container.Factories)
                {
                    if (factory.Value == null || factory.Value.Original == null)
                    {
                        Debug.LogWarning($"Factory {factory.Key} is null or has no original object.");
                        continue;
                    }
                    
                    RegisterFactory(factory.Value, factory.Key.ToString());
                }
            }

        }
        
        /// <summary>
        /// 등록된 팩토리와 풀을 모두 제거
        /// </summary>
        public void Clear()
        {
            foreach (var pool in _pools)
            {
                pool.Value.Clear();
            }
            _pools.Clear();
            _typePools.Clear();
            _factories.Clear();
        }
        
        /// <summary>
        /// 해당 Poolable을 생성하는 팩토리를 등록.
        /// </summary>
        /// <param name="poolable"></param>
        public void RegisterFactory(Poolable poolable)
        {
            RegisterFactory(poolable, poolable.name);
        }
        
        /// <summary>
        /// 해당 Poolable을 생성하는 팩토리를 등록.
        /// type 지정 가능
        /// </summary>
        /// <param name="poolable"></param>
        /// <param name="type"></param>
        public void RegisterFactory(Poolable poolable, string type)
        {
            if (poolable == null)
            {
                Debug.LogError("Poolable cannot be null.");
                return;
            }
            
            if (_factories.ContainsKey(type))
            {
                Debug.LogWarning($"Factory for {type} already exists. Overwriting.");
            }
            
            ICloneFactory<Poolable> cloneFactory = ScriptableObject.CreateInstance<PoolableFactorySo>();
            cloneFactory.Original = poolable;
            RegisterFactory(cloneFactory, type);
        }
        
        /// <summary>
        /// 팩토리를 등록합니다.
        /// </summary>
        /// <param name="cloneFactory"></param>
        /// <param name="type"></param>
        public void RegisterFactory(ICloneFactory<Poolable> cloneFactory, string type)
        {
            if (cloneFactory == null)
            {
                Debug.LogError("Factory cannot be null.");
                return;
            }
            
            if (_factories.ContainsKey(cloneFactory.Original.name))
            {
                Debug.LogWarning($"Factory for {cloneFactory.Original.name} already exists. Overwriting.");
            }
            
            _factories[type] = cloneFactory;
            _pools[type] =
                new ObjectPool<Poolable>(
                    cloneFactory.Create,
                    ActionOnGet,
                    ActionOnRelease,
                    null,
                    true,
                    _initialSize,
                    _maxSize
                );
            _typePools[cloneFactory.Original.GetType()] = _pools[type];
        }

        private void ActionOnGet(Poolable poolable)
        {
            poolable.transform.SetParent(_tempPoolParent,false);
            poolable.OnGetFromPool();
        }
        
        private void ActionOnRelease(Poolable poolable)
        {
            poolable.transform.SetParent(_RootTransform, false);
            poolable.OnReturnToPool();
        }
        
        /// <summary>
        /// Poolable을 반환
        /// </summary>
        /// <param name="poolable"></param>
        public void Release(Poolable poolable)
        {
            if (poolable._pool != null)
            {
                poolable._pool.Release(poolable);
            }
            else
            {
                Debug.LogWarning($"Poolable {poolable.name}이 Pool에 속하지 않음. 파괴.");
                Object.Destroy(poolable.gameObject);
            }
        }
        
        /// <summary>
        /// Poolable을 가져옴.
        /// 만약 해당 Poolable이 등록되어 있지 않다면, 새로운 팩토리를 등록하고 가져온다.
        /// </summary>
        /// <param name="original"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public Poolable GetFromOriginal(Poolable original, Transform parent = null)
        {
            if (original == null)
            {
                Debug.LogError("Poolable cannot be null.");
                return null;
            }
            
            _tempPoolParent = parent;
            if (_pools.TryGetValue(original.name, out IObjectPool<Poolable> pool))
            {
                var instance = pool.Get();
                instance._pool = pool;
                return instance;
            }
            else
            {
                Debug.Log($"No pool registered for {original.name}. Registering new pool.");
                RegisterFactory(original);
                return Get(original.name, parent);
            }
        }
        /// <summary>
        /// Poolable을 가져옴.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public Poolable Get(Poolables type, Transform parent = null)
        {
            return Get(type.ToString(), parent);
        }
        
        /// <summary>
        /// Poolable을 가져옴.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public Poolable Get(string type, Transform parent = null)
        {
            _tempPoolParent = parent;
            var poolable = _pools[type].Get();
            poolable._pool = _pools[type];
            return poolable;
        }
        
        /// <summary>
        /// Poolable을 가져옴.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="parent"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Get<T>(Poolables type, Transform parent = null) where T : MonoBehaviour
        {
            return Get<T>(type.ToString(), parent);
        }
        
        /// <summary>
        /// Poolable을 가져옴.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="parent"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Get<T>(string type, Transform parent = null) where T : MonoBehaviour
        {
            var poolable = Get(type, parent);
            if (poolable.TryGetComponent(out T tPoolable))
            {
                return tPoolable;
            }
            else if (poolable is T tPoolableDirect)
            {
                return tPoolableDirect;
            }
            else
            {
                Debug.LogError(
                    $"Requested type {typeof(T)} do`es not match the poolable type {poolable.GetType()} from pool {type}");
                return null;
            }
        }
        
        
        /// <summary>
        /// 타입으로 Poolable을 가져옴.
        /// </summary>
        /// <param name="parent"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Get<T> (Transform parent = null) where T : MonoBehaviour, IPoolableSubComponent
        {
            var pool = _typePools[typeof(T)];
            _tempPoolParent = parent;
            var poolable = pool.Get();
            poolable._pool = pool;
            return poolable.GetComponent<T>();
        }
        
        /// <summary>
        /// Poolable의 원본을 가져옴.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public Poolable GetOriginal(Poolables type)
        {
            return GetOriginal(type.ToString());
        }
        
        /// <summary>
        /// Poolable의 원본을 가져옴.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public Poolable GetOriginal(string type)
        {
            if (_factories.TryGetValue(type, out ICloneFactory<Poolable> factory))
            {
                Poolable original = factory.Original;
                return original;
            }
            else
            {
                Debug.LogError($"No factory registered for type {type}. Cannot get original.");
                return null;
            }
        }
        /// <summary>
        /// Poolable의 원본을 가져옴.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="original"></param>
        /// <returns></returns>
        public bool TryGetOriginal(string type, out Poolable original)
        {
            if (_factories.TryGetValue(type, out ICloneFactory<Poolable> factory))
            {
                original = factory.Original;
                return true;
            }
            else
            {
                original = null;
                return false;
            }
        }
    }
}