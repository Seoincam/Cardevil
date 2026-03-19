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
        
        public void RuntimeInitialize()
        {
            _map = relics.ToDictionary(r => r.Data.Id, r => r);    
        }

        public RelicSO Get(string id)
        {
            return _map.GetValueOrDefault(id);
        }
    }
}