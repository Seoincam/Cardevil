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
        [FormerlySerializedAs("hoverImage")] [SerializeField] private Image overlayImage;
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
                if(dungeonNode != null)
                {
                    dungeonNode.OnStateChanged -= OnDungeonNodeStateChanged;
                }
                dungeonNode = value;
                if(dungeonNode != null)
                {
                    dungeonNode.OnStateChanged += OnDungeonNodeStateChanged;
                    UpdateView();
                }
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
            // 들어갈 수 있는 경우에만
            if (dungeonNode.State != NodeState.Available)
            {
                Debug.LogWarning($"[DungeonNodeUI] Cannot enter node {dungeonNode.NodeId} - state is {dungeonNode.State}");
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
        
        private void OnDungeonNodeStateChanged(NodeState newState)
        {
            UpdateView();
        }

        public void UpdateView()
        {
            if (dungeonNode == null)
            {
                Debug.LogWarning($"[DungeonNodeUI] UpdateView called but dungeonNode is null on {name}");
                return;
            }
            
            Debug.Log($"[DungeonNodeUI] UpdateView - Node {dungeonNode.NodeId}, State: {dungeonNode.State}, GameObject active: {gameObject.activeInHierarchy}");
            
            DungeonNodeSettingSO.SpriteSet spriteSet = setting.GetSpriteSet(dungeonNode.Type);
            switch (dungeonNode.State)
            {
                case NodeState.Locked:
                    nodeImage.sprite = spriteSet.Inactive;
                    nodeText.text = "";
                    _button.interactable = false;
                    _button.gameObject.SetActive(false);
                    Debug.Log($"[DungeonNodeUI] Node {dungeonNode.NodeId} - Button set to INACTIVE (Locked)");
                    SetOverlaySprite(null);
                    break;
                case NodeState.Available:
                    nodeImage.sprite = spriteSet.Active;
                    _button.interactable = true;
                    _button.gameObject.SetActive(true);
                    Debug.Log($"[DungeonNodeUI] Node {dungeonNode.NodeId} - Button set to ACTIVE (Available)");
                    SetOverlaySprite(null);
                    // nodeText.text = dungeonNode.NodeId.ToString();
                    break;
                case NodeState.Current:
                    nodeImage.sprite = spriteSet.Active;
                    _button.interactable = false;
                    _button.gameObject.SetActive(true);
                    Debug.Log($"[DungeonNodeUI] Node {dungeonNode.NodeId} - Button set to ACTIVE but not interactable (Current)");
                    SetOverlaySprite(null);
                    break;
                case NodeState.Completed:
                    nodeImage.sprite = spriteSet.Completed;
                    nodeText.text = "";
                    _button.interactable = false;
                    _button.gameObject.SetActive(true);
                    Debug.Log($"[DungeonNodeUI] Node {dungeonNode.NodeId} - Button set to ACTIVE but not interactable (Completed)");
                    SetOverlaySprite(spriteSet.CompletedOverlay);
                    
                    break;
            }
        }

        private void SetOverlaySprite(Sprite sprite)
        {
            if(sprite == null)
            {
                overlayImage.gameObject.SetActive(false);
                return;
            }
            overlayImage.gameObject.SetActive(true);
            overlayImage.sprite = sprite;
        }
    }
}