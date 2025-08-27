using System;
using System.Collections.Generic;
using UnityEngine;

namespace Cardevil.Dungeon.UI
{
    public class DungeonStageUI : MonoBehaviour
    {
        [SerializeField] private int dungeonId = -1;
        [Header("References")]
        [SerializeField] private DungeonUI dungeonUI;
        [SerializeField] private DungeonNodeUI nodeUiPrefab;
        [SerializeField] private List<DungeonNodeUI> nodeUis = new List<DungeonNodeUI>();
        
        public int DungeonId => dungeonId;
        public Dungeon Dungeon => Managers.Dungeon.GetDungeon(dungeonId);


        private void Awake()
        {
            if (dungeonUI == null)
            {
                dungeonUI = GetComponentInParent<DungeonUI>();
                if (dungeonUI == null)
                {
                    Debug.LogError("DungeonUI reference is not assigned and not found in parent.");
                }
            }
        }

        [ContextMenu("Create Node UI")]
        public void CreateNodeUI()
        {
            if (nodeUiPrefab == null)
            {
                Debug.LogError("Node UI Prefab is not assigned.");
                return;
            }

            DungeonNodeUI newNodeUI = Instantiate(nodeUiPrefab, transform);
            newNodeUI.InitRef(dungeonUI, this);
            nodeUis.Add(newNodeUI);
            newNodeUI.name = $"NodeUI_{nodeUis.Count}";
        }

        [ContextMenu("Create Node UIs By Dungeon")]
        public void CreateNodeUIsByDungeon()
        {
            if (Dungeon == null)
            {
                Debug.LogError($"No Dungeon found for dungeon ID {dungeonId}");
                return;
            }

            // Clear existing node UIs
            foreach (var nodeUi in nodeUis)
            {
                if (nodeUi != null)
                {
                    DestroyImmediate(nodeUi.gameObject);
                }
            }
            nodeUis.Clear();
            if (nodeUiPrefab == null)
            {
                Debug.LogError("Node UI Prefab is not assigned.");
                return;
            }

            // Create new node UIs based on the dungeon's nodes
            foreach (var dungeonNode in Dungeon.Nodes)
            {
                DungeonNodeUI newNodeUI = Instantiate(nodeUiPrefab, transform);
                newNodeUI.InitRef(dungeonUI, this);
                nodeUis.Add(newNodeUI);
                newNodeUI.name = $"NodeUI_{dungeonNode.NodeId}";
                
                var nodeIdField = newNodeUI.GetType().GetField("nodeId", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (nodeIdField != null)
                {
                    nodeIdField.SetValue(newNodeUI, dungeonNode.NodeId);
                }
                else
                {
                    Debug.LogWarning("Could not set nodeId on the instantiated Node UI.");
                }
            }
        }

        private void OnEnable()
        {
            foreach (DungeonNodeUI nodeUi in nodeUis)
            {
                nodeUi.InitializeNode();
            }
            
            foreach (DungeonNodeUI nodeUi in nodeUis)
            {
                nodeUi.InitializeLine();
            }
        }
        
        public DungeonNodeUI GetNodeUI(int nodeId)
        {
            foreach (DungeonNodeUI nodeUi in nodeUis)
            {
                if (nodeUi.DungeonId == dungeonId)
                {
                    return nodeUi;
                }
            }
            return null;
        }
    }
}