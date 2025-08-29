using Cardevil.Attributes;
using Cardevil.Dungeon;
using Cardevil.Dungeon.UI;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Cardevil.Dungeon.UI
{
    public class DungeonNodeUI : MonoBehaviour
    {
        [Header("Visual")]
        [SerializeField] private Animator nodeAnimator;
        [SerializeField] private Image nodeImage;
        [SerializeField] private TextMeshProUGUI nodeText;
        [Header("UI")]
        [SerializeField] private DungeonUI dungeonUI;
        [SerializeField] private DungeonChapterUI dungeonChapterUI;
        [Header("Dungeon Node Info")]
        [SerializeField,VisibleOnly] private DungeonNode dungeonNode;
        [Space]
        [Header("Setting")]
        [SerializeField] private int nodeId = -1;
        
        
        public int DungeonId => dungeonChapterUI.DungeonId;
        
        public void InitRef(DungeonUI dungeonUI, DungeonChapterUI chapterUI)
        {
            this.dungeonUI = dungeonUI;
            this.dungeonChapterUI = chapterUI;
        }

        public void InitializeNode()
        {
            if (nodeId <= 0)
            {
                Debug.LogError($"Node ID{nodeId} is not set or invalid.");
                return;
            }
            dungeonNode = dungeonChapterUI.Dungeon.Nodes[nodeId - 1];
            name = $"Node_{dungeonNode.NodeId}_{dungeonNode.Type}";
            if(nodeText)
                nodeText.text = dungeonNode.Type.ToString();
        }

        public void InitializeLine()
        {
            foreach (DungeonNode dungeonNodeNext in dungeonNode.NextNodes)
            {
                DungeonNodeUI nextNodeUI = dungeonChapterUI.GetNodeUI(dungeonNodeNext.NodeId);
                if (nextNodeUI == null)
                {
                    Debug.LogError($"No DungeonNodeUI found for node ID {dungeonNodeNext.NodeId}");
                    continue;
                }
                
                Debug.Log($"Init Node lies : {name} -> {nextNodeUI.name}");
                
            }
        }
    }
}