using Cardevil.DataStructure;
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
        [SerializeField] private SerialzableDict<PoolManager.Poolables, PoolableFactorySO> _factories = new ();
        
        public SerialzableDict<PoolManager.Poolables, PoolableFactorySO> Factories
        {
            get => _factories;
        }

        [ContextMenu("Register All")]
        public void RegisterAll()
        {
            PoolableFactorySO[] allFactories = Resources.FindObjectsOfTypeAll<PoolableFactorySO>();
            foreach (var factory in allFactories)
            {
                if (factory == null || factory.Original == null)
                {
                    Debug.LogWarning($"Factory {factory.name} is null or has no original object.");
                    continue;
                }

                if(Enum.TryParse(typeof(PoolManager.Poolables), factory.Original.name, out var poolableEnum))
                {
                    PoolManager.Poolables poolableType = (PoolManager.Poolables)poolableEnum;
                    _factories[poolableType] = factory;
                }
                else
                {
                    _factories[(PoolManager.Poolables)Random.Range(0, 10000)] = factory;
                }
  
            }
        }
    }
}