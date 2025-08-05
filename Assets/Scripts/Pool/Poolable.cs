using System;
using UnityEngine;
using UnityEngine.Pool;

namespace Cardevil.Pools
{
    public class Poolable : MonoBehaviour
    {
        internal IObjectPool<Poolable> _pool;
        

        public event Action OnGet;
        public event Action OnBeforeRelease;
        
        public void Release()
        {
            if (_pool == null)
            {
                Destroy(gameObject);
                return;
            }
            OnBeforeRelease?.Invoke();   
            _pool.Release(this);
        }
        
        internal void OnGetFromPool()
        {
            OnGet?.Invoke();
            gameObject.SetActive(true);
        }
    
        internal void OnReturnToPool()
        {
            OnBeforeRelease?.Invoke();
            gameObject.SetActive(false);
        }
    }

}