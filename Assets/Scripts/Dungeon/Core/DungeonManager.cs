using Cardevil.Attributes;
using Cardevil.DebugConsole;
using Cardevil.Dungeon.Build;
using Cardevil.Dungeon.UI;
using Cardevil.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;
using Console = Cardevil.DebugConsole.Console;
using Object = UnityEngine.Object;

namespace Cardevil.Dungeon.Core
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
                    dungeonUI = Object.FindAnyObjectByType<DungeonUI>(FindObjectsInactive.Include);
                    if (dungeonUI == null)
                    {
                        LogEx.LogError("No DungeonUI found in the scene.");
                    }
                }
                return dungeonUI;
            }
        }

        public int CurrentDungeonIndex
        {
            get => currentDungeonIndex;
            set
            {
                Dungeon dungeon = GetDungeon(value);
                if (dungeon == null)
                {
                    LogEx.LogError($"[DungeonManager] Dungeon with index {value} does not exist.");
                    return;
                }
                currentDungeonIndex = value;
                if (UI == null)
                {
                    LogEx.LogWarning("[DungeonManager] DungeonUI is not assigned.");
                    return;
                }

                UI.UpdateShowingDungeon(currentDungeonIndex);
            }
        }
        public DungeonConfigurationSO CurrentDungeonConfiguration => dungeonConfigurations[currentDungeonIndex];
        public Dungeon CurrentDungeon => GetDungeon(currentDungeonIndex);
        public DungeonNode CurrentNode => currentNode;

        
        public void Init()
        {
            LogEx.Log("Initializing Dungeon Manager...");
            currentDungeonIndex = 1;
            
           
            // TODO : DungeonConfig 로드
            
            // UI 기반으로 던전 생성
            CreateDungeons();
            foreach (Dungeon dungeon in dungeons)
            {
                dungeon.Initialize();
            }
            UI.Initialize();
        }

        private void CreateDungeons()
        {
            dungeons.Clear();
            LogEx.Log("Creating dungeons from UI...");
            var buildHelpers = UI.GetComponentsInChildren<DungeonBuildHelperUI>();
            foreach (DungeonBuildHelperUI buildHelper in buildHelpers)
            {
                DungeonChapterUI chapterUI = buildHelper.GetComponent<DungeonChapterUI>();
                Dungeon dungeon = buildHelper.BuildDungeon();
                dungeon.dungeonId = chapterUI.DungeonId;
                dungeons.Add(dungeon);
                LogEx.Log($"Dungeon {dungeon.DungeonId} created with {dungeon.Nodes.Count} nodes.");
                LogEx.Log(dungeon.GetDebugString());
            }
        }
        
        private void AssignDungeonUIs()
        {
            if (UI == null)
            {
                Debug.LogWarning("[DungeonManager] DungeonUI is not assigned.");
                return;
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
        
        
        [ConsoleCommand("dungeonSetCurrent", "Sets the current dungeon by index.", "dungeonSetCurrent <index>", new string[]{"1","2","3"})]
        public static void SetCurrentDungeonCommand(int idx){
            DungeonManager dm = Managers.Dungeon;
            Dungeon dungeon = dm.GetDungeon(idx);
            if (dungeon == null)
            {
                Console.MessageError($"Dungeon with index {idx} does not exist.");
                return;
            }
            dm.CurrentDungeonIndex = idx;
            Console.Message($"Current dungeon set to index {idx}.");
        }
    }
}