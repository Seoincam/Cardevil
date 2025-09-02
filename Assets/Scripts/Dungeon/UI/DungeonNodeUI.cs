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
        [Header("Internal References")]
        [SerializeField] private Button _button;
        [Header("Visual")]
        [SerializeField] private Animator nodeAnimator;
        [SerializeField] private Image nodeImage;
        [SerializeField] private TextMeshProUGUI nodeText;
        [Space]
        [Header("External References")]
        [SerializeField] private DungeonUI dungeonUI;
        [SerializeField] private DungeonChapterUI dungeonChapterUI;
        [Header("Dungeon Node Info")]
        [SerializeField,VisibleOnly] private DungeonNode dungeonNode;
        [Space]
        [Header("Variables")]
        [SerializeField] private int nodeId = -1;

        private LineRenderer lineRenderer;
        
        public int DungeonId => dungeonChapterUI.DungeonId;

        public DungeonNode DungeonNode
        {
            get => dungeonNode;
            set
            {
                dungeonNode = value;
            }
        }
        
        public void InitRef(DungeonUI dungeonUI, DungeonChapterUI chapterUI)
        {
            this.dungeonUI = dungeonUI;
            this.dungeonChapterUI = chapterUI;
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

        public void Awake()
        {
            _button.onClick.AddListener(OnClickButton);
        }

        public void OnClickButton()
        {
            Debug.Log($"Clicked on node {dungeonNode.NodeId}");
            Managers.Dungeon.EnterNode(dungeonNode);
        }


        private void OnValidate()
        {

        }
    }
}