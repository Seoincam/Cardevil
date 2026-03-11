using UnityEngine;

namespace Cardevil.Core.Systems.Pool
{
    [CreateAssetMenu(fileName = "PoolableFactory", menuName = "Pool/PoolableFactory")]
    public class PoolableFactorySo : ScriptableObject, ICloneFactory<Poolable>
    {
        [SerializeField] private Poolable _original;
        public virtual Poolable Create()
        {
            return Instantiate(_original);
        }
        
        public Poolable Original
        {
            get => _original;
            set => _original = value;
        }
    }

}