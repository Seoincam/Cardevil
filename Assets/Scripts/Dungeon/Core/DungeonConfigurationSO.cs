using System.Collections.Generic;
using UnityEngine;

namespace Cardevil.Dungeon
{
    [CreateAssetMenu(fileName = "DungeonConfiguration", menuName = "ScriptableObjects/DungeonConfiguration", order = 1)]
    public class DungeonConfigurationSO : ScriptableObject
    {
        [Header("Dungeon Configuration")]
        [SerializeField] private int dungeonId;
        [SerializeField] private List<DungeonNodePreset> nodePresets = new List<DungeonNodePreset>();

        public int DungeonId => dungeonId;
        public List<DungeonNodePreset> NodePresets => nodePresets;
    }
}
