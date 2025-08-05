using System.Collections.Generic;
using UnityEngine;

namespace Cardevil.DataStructure
{
    [System.Serializable]
    public class SerializableReferenceDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        [SerializeField] private TKey defaultKey;
    
        [System.Serializable]
        public class Data
        {
            [SerializeField] public TKey key;
            [SerializeReference] public TValue value;
        }
    
        [SerializeField] private List<Data> data = new List<Data>();
    
        public void OnBeforeSerialize()
        {
            data.Clear();
            foreach (var pair in this)
            {
                data.Add(new Data()
                {
                    key = pair.Key,
                    value = pair.Value
                });
            }
        }
    
        public void OnAfterDeserialize()
        {
            this.Clear();
            foreach (var pair in data)
            {
                if (this.ContainsKey(pair.key))
                {
                    if (!ContainsKey(defaultKey))
                    {
                        this[defaultKey] = this[pair.key];
                    }
                }
                this[pair.key] = pair.value;
            }
        }
    
    }
}