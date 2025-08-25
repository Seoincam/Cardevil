using System;
using UnityEngine;
using UnityEngine.Pool;

namespace Cardevil.Pools
{
    public class Poolable : MonoBehaviour
    {
        internal IObjectPool<Poolable> _pool;
        

        public event Action OnGet;
        public event Action OnRelease;
        public event Action OnReleaseOnce;
       
        
        public void Release()
        {
            if (_pool == null)
            {
                OnRelease?.Invoke();
                OnReleaseOnce?.Invoke();
                Destroy(gameObject);
                return;
            }
            _pool.Release(this);
        }
        
        internal void OnGetFromPool()
        {
            OnGet?.Invoke();
            gameObject.SetActive(true);
        }
    
        internal void OnReturnToPool()
        {
            OnRelease?.Invoke();
            OnReleaseOnce?.Invoke();
            OnReleaseOnce = null;
            gameObject.SetActive(false);
        }
    }

}