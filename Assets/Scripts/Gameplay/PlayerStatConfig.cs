using System;
using System.Collections.Generic;
using UnityEngine;

namespace Cardevil.Gameplay
{
    [CreateAssetMenu(menuName = "Gameplay/PlayerStatConfig")]
    public class PlayerStatConfig : ScriptableObject
    {
        [SerializeField] private List<StatEntry> entries;
        
        public IReadOnlyList<StatEntry> Entries => entries;
        
        [Serializable]
        public struct StatEntry
        {
            [field: SerializeField] public PlayerStatType Type { get; private set; }
            [field: SerializeField] public int Value { get; private set; }
        }
    }
}