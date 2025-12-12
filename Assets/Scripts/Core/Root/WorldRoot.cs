using Cardevil.Dungeon;
using UnityEngine;

namespace Cardevil.Core.Root
{
    public class WorldRoot : MonoBehaviour
    {
        public static WorldRoot Instance { get; private set; }
        
        [field: SerializeField] public DungeonManager Dungeon { get; private set; }

        private void Awake()
        {
            if (Instance)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            
            Init();
        }

        public void Init()
        {
            Dungeon = new DungeonManager();
            Dungeon.Init();
        }
    }
}