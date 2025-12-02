using Cardevil.Attributes;
using Cardevil.Dungeon;
using Cardevil.Dungeon.UI;
using Cardevil.Utils;
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
        [Header("Visual References")]
        [SerializeField] private Animator nodeAnimator;
        [SerializeField] private Image nodeImage;
        [SerializeField] private TextMeshProUGUI nodeText;
        [SerializeField] private Image hoverImage;
        [Space]
        [Header("External References")]
        [SerializeField] private DungeonUI dungeonUI;
        [SerializeField] private DungeonChapterUI dungeonChapterUI;
        [Header("Settings")]
        [SerializeField] private DungeonNodeSettingSO setting = null;
        [Header("Dungeon Node Info")]
        [SerializeReference,VisibleOnly] private DungeonNode dungeonNode;
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
            if (dungeonNode == null)
            {
                Debug.LogWarning($"[DungeonNodeUI] Cannot initialize lines - dungeonNode is null on {name}");
                return;
            }

            foreach (DungeonNode dungeonNodeNext in dungeonNode.NextNodes)
            {
                DungeonNodeUI nextNodeUI = dungeonChapterUI.GetNodeUI(dungeonNodeNext.NodeId);
                if (nextNodeUI == null)
                {
                    Debug.LogError($"No DungeonNodeUI found for node ID {dungeonNodeNext.NodeId}");
                    continue;
                }
                
                Debug.Log($"Init Node lines : {name} -> {nextNodeUI.name}");
            }
        }

        public void Awake()
        {
            _button.onClick.AddListener(OnClickButton);
            if (setting == null)
            {
                setting = DungeonNodeSettingSO.Default;
            }
        }

        public void OnClickButton()
        {
            if (dungeonNode == null)
            {
                Debug.LogWarning($"[DungeonNodeUI] Cannot click node - dungeonNode is null on {name}");
                return;
            }
            
            Debug.Log($"Clicked on node {dungeonNode.NodeId}");
            Managers.Dungeon.EnterNode(dungeonNode);
        }

        private void Reset()
        {
            setting = DungeonNodeSettingSO.Default;
        }

        private void OnValidate()
        {
            if (setting == null)
            {
                setting = DungeonNodeSettingSO.Default;
            }
        }

        public void UpdateView()
        {
            DungeonNodeSettingSO.SpriteSet spriteSet = setting.NodeTypeToSpriteSet[dungeonNode.Type];
            switch (dungeonNode.State)
            {
                case NodeState.Locked:
                    nodeImage.sprite = spriteSet.Inactive;
                    nodeText.text = "";
                    _button.interactable = false;
                    break;
                case NodeState.Available:
                    nodeImage.sprite = spriteSet.Active;
                    _button.interactable = true;
                    // nodeText.text = dungeonNode.NodeId.ToString();
                    break;
                case NodeState.Current:
                    nodeImage.sprite = spriteSet.Active;
                    _button.interactable = true;
                    break;
                case NodeState.Completed:
                    nodeImage.sprite = spriteSet.Inactive;
                    nodeText.text = "";
                    _button.interactable = false;
                    break;
            }
        }
    }
}