using Cardevil.Gameplay.Relics.Core;
using System.Collections.Generic;
using UnityEngine;

namespace Cardevil.Gameplay.Relics
{
    [CreateAssetMenu(fileName = "Relic Database", menuName = "Relic/Database")]
    public class RelicDatabase : ScriptableObject
    {
        [SerializeField] private List<Relic> relics;

        public IReadOnlyList<Relic> Relics => relics;
    }
}