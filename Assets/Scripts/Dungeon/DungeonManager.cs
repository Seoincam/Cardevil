using Cardevil.Attributes;
using Cardevil.Dungeon.DungeonFactories;
using Cardevil.Dungeon.UI;
using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Cardevil.Dungeon
{
    [Serializable]
    public class DungeonManager
    {
        [SerializeField] private List<DungeonConfigurationSO> dungeonConfigurations = new List<DungeonConfigurationSO>();
        
        [SerializeReference] private List<Dungeon> dungeons = new List<Dungeon>();
        [SerializeField] private DungeonUI dungeonUI = null;
        [SerializeField, VisibleOnly] private DungeonNode currentNode;
        private int currentDungeonIndex = -1;

        public DungeonUI UI
        {
            get
            {
                if (dungeonUI == null)
                {
                    dungeonUI = Object.FindAnyObjectByType<DungeonUI>();
                    if (dungeonUI == null)
                    {
                        Debug.LogError("No DungeonUI found in the scene.");
                    }
                }
                return dungeonUI;
            }
        }
        public int CurrentDungeonIndex => currentDungeonIndex;
        public DungeonConfigurationSO CurrentDungeonConfiguration => dungeonConfigurations[currentDungeonIndex];
        public Dungeon CurrentDungeon => GetDungeon(currentDungeonIndex);
        public DungeonNode CurrentNode => currentNode;

        
        public void Init()
        {
            currentDungeonIndex = 1;
            
           
            // TODO : DungeonConfig 로드
            
            // UI 기반으로 던전 생성
            CreateDungeons();
            
        }

        private void CreateDungeons()
        {
            dungeons.Clear();
            Debug.Log("[DungeonManager] Creating dungeons from UI...");
            var buildHelpers = UI.GetComponentsInChildren<DungeonBuildHelperUI>();
            foreach (DungeonBuildHelperUI buildHelper in buildHelpers)
            {
                Dungeon dungeon = buildHelper.BuildDungeon();
                dungeons.Add(dungeon);
                Debug.Log($"[DungeonManager] Dungeon {dungeon.DungeonId} created with {dungeon.Nodes.Count} nodes.");
                Debug.Log(dungeon.GetDebugString());
            }
        }

        public Dungeon GetDungeon(int id)
        {
            foreach (var dungeon in dungeons)
            {
                if (dungeon.DungeonId == id)
                {
                    return dungeon;
                }
            }
            return null;
        }
        
        
        public void EnterNode(DungeonNode node)
        {
            if (node == null)
            {
                Debug.LogWarning("No dungeon node provided.");
                return;
            }
            if(node.Preset == null)
            {
                Debug.LogWarning("No preset assigned to this dungeon node.");
                return;
            }
            Debug.Log($"Entering node: {node.NodeId}");
            currentNode = node;
            node.Preset.OnEnter();
        }
    }
}