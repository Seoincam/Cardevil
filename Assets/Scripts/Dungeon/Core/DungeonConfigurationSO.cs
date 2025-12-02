using System.Collections.Generic;
using UnityEngine;

namespace Cardevil.Dungeon
{
    [CreateAssetMenu(fileName = "DungeonConfiguration", menuName = "ScriptableObjects/DungeonConfiguration", order = 1)]
    public class DungeonConfigurationSO : ScriptableObject
    {
        [Header("Dungeon Configuration")]
        [SerializeField] private int dungeonId;
        [SerializeField] private List<DungeonNodeBehaviour> nodePresets = new List<DungeonNodeBehaviour>();

        public int DungeonId => dungeonId;
        public List<DungeonNodeBehaviour> NodePresets => nodePresets;
    }
}
