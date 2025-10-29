using Cardevil.DataStructure;
using Cardevil.DataStructure.Serializables;
using Cardevil.Manager;
using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Cardevil.Pools
{
    /// <summary>
    /// PoolableFactoryContainerSO는 PoolableFactorySO를 관리하는 ScriptableObject.
    /// 해당 컨테이너에 등록된 팩토리들은 PoolManager에서 자동으로 등록됨.
    /// </summary>
    /// <remarks>
    /// NetworkPrebab SO랑 비슷한 역할임.
    /// </remarks>
    [CreateAssetMenu(fileName = "PoolableFactoryContainer", menuName = "Pool/PoolableFactoryContainer")]
    public class PoolableFactoryContainerSO : ScriptableObject
    {
        [SerializeField] private SerializableDictionary<Poolables, PoolableFactorySo> _factories = new ();
        
        public SerializableDictionary<Poolables, PoolableFactorySo> Factories
        {
            get => _factories;
        }

        [ContextMenu("Register All")]
        public void RegisterAll()
        {
            PoolableFactorySo[] allFactories = Resources.FindObjectsOfTypeAll<PoolableFactorySo>();
            foreach (var factory in allFactories)
            {
                if (factory == null || factory.Original == null)
                {
                    Debug.LogWarning($"Factory {factory.name} is null or has no original object.");
                    continue;
                }

                if(Enum.TryParse(typeof(Poolables), factory.Original.name, out var poolableEnum))
                {
                    Poolables poolableType = (Poolables)poolableEnum;
                    _factories[poolableType] = factory;
                }
                else
                {
                    _factories[(Poolables)Random.Range(0, 10000)] = factory;
                }
  
            }
        }
    }
}