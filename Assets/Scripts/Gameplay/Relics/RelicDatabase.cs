using Cardevil.Gameplay.Relics.Core;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Cardevil.Gameplay.Relics
{
    [CreateAssetMenu(fileName = "Relic Database", menuName = "Relic/Database")]
    public class RelicDatabase : ScriptableObject
    {
        public List<RelicSO> relics;
        
        private Dictionary<string, RelicSO> _map;
        
        public IReadOnlyDictionary<string, RelicSO> Map => _map;
        
        public void RuntimeInitialize()
        {
            _map = relics.ToDictionary(r => r.Data.Id, r => r);    
        }

        public RelicDefinition Get(string id)
        {
            return _map.GetValueOrDefault(id)?.Data;
        }
        
        public IEnumerable<RelicDefinition> GetAll()
        {
            return _map.Values.Select(r => r.Data);
        }
    }
}