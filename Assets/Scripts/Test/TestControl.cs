using Cardevil.Manager;
using Cardevil.Pools;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Test
{
    /// <summary>
    /// Context menu 등을 통한 테스트용 MonoBehaviour
    /// </summary>
    public class TestControl : MonoBehaviour
    {
        
        [Header("Pool Test")]
        public List<Cardevil.Pools.Poolable> poolables = new List<Cardevil.Pools.Poolable>();
        [FormerlySerializedAs("poolableName")] public string resourcePoolableName = "TestPoolable";
        public PoolManager.Poolables poolableType = PoolManager.Poolables.TestPoolable;
        [ContextMenu("Get Test Poolable")]
        public void GetTestPoolableFromResource()
        {
            // PoolableFactorySO를 통해 Poolable 객체를 가져오는 테스트
            var poolable = Managers.Resource.Instantiate(resourcePoolableName, transform).GetComponent<Cardevil.Pools.Poolable>();
            if (poolable != null)
            {
                Debug.Log("Poolable 객체를 성공적으로 가져왔습니다.");
                poolables.Add(poolable);
            }
            else
            {
                Debug.LogError("Poolable 객체를 가져오는 데 실패했습니다.");
            }
        }
        
        [ContextMenu("Get Test Poolable From Pool")]
        public void GetTestPoolableFromPool()
        {
            // PoolManager를 통해 Poolable 객체를 가져오는 테스트
            Poolable poolable = Managers.Pool.Get(poolableType);
            if (poolable != null)
            {
                Debug.Log("Poolable 객체를 성공적으로 가져왔습니다.");
                poolables.Add(poolable);
            }
            else
            {
                Debug.LogError("Poolable 객체를 가져오는 데 실패했습니다.");
            }
        }
        
        [ContextMenu("Release Test Poolable")]
        public void ReleaseTestPoolable()
        {
            // Poolable 객체를 반환하는 테스트
            if (poolables.Count > 0)
            {
                var poolable = poolables[0];
                poolable.Release();
                poolables.RemoveAt(0);
                Debug.Log("Poolable 객체를 성공적으로 반환했습니다.");
            }
            else
            {
                Debug.LogError("반환할 Poolable 객체가 없습니다.");
            }
        }
    }
}