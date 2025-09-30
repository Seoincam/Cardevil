using System;
using System.Collections.Generic;
using UnityEngine;

namespace Cardevil.Core
{
    /// <summary>
    /// 런타임에 초기화 되는 ScriptableObject
    /// </summary>
    public abstract class RuntimeSO : UnityEngine.ScriptableObject
    {
        private static List<RuntimeSO> _instances = new List<RuntimeSO>();
        private static bool _initialized = false;


        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Init()
        {
            if (_initialized) return;
            _initialized = true;
            foreach (var instance in _instances)
            {
                instance.OnReset();
            }
        }
        
        public virtual void OnEnable()
        {
            if (!_instances.Contains(this))
            {
                _instances.Add(this);
            }
            if (_initialized)
            {
                OnReset();
            }
        }
        
        public virtual void OnDisable()
        {
            if (_instances.Contains(this))
            {
                _instances.Remove(this);
            }
        }
        public abstract void OnReset();
        
    }
}