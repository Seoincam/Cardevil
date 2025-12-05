using Cardevil.Attributes;
using Cardevil.Utils;
using Cysharp.Threading.Tasks;
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
        [SerializeField] private DungeonNodeSettingSO setting;
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
                }
            }
        }
        
        public void InitRef(DungeonUI ui, DungeonChapterUI chapterUI)
        {
            this.dungeonUI = ui;
            this.dungeonChapterUI = chapterUI;
        }

        public void InitializeLine()
        {
            if (dungeonNode == null)
            {
                LogEx.LogWarning($"dungeonNode is null");
                return;
            }

            foreach (DungeonNode dungeonNodeNext in dungeonNode.NextNodes)
            {
                DungeonNodeUI nextNodeUI = dungeonChapterUI.GetNodeUI(dungeonNodeNext.NodeId);
                if (nextNodeUI == null)
                {
                    LogEx.LogError($"No DungeonNodeUI found for node ID {dungeonNodeNext.NodeId}");
                }
            }
        }

        public void Awake()
        {
            if (_button != null)
            {
                _button.gameObject.SetActive(true);
                _button.onClick.AddListener(OnClickButton);
            }
            
            if (setting == null)
            {
                setting = DungeonNodeSettingSO.Default;
            }
        }

        private void OnEnable()
        {
            if (_button != null && !_button.gameObject.activeSelf)
            {
                _button.gameObject.SetActive(true);
            }
        }

        private void Start()
        {
            if (dungeonNode != null)
            {
                UpdateView();
            }
        }

        public void OnClickButton()
        {
            if (dungeonNode == null)
            {
                LogEx.LogWarning($"dungeonNode is null");
                return;
            }
            
            if (dungeonNode.State != NodeState.Available)
            {
                LogEx.LogWarning($"Cannot enter node {dungeonNode.NodeId} - state is {dungeonNode.State}");
                return;
            }
            
            var dungeonManager = Managers.Dungeon;
            if (dungeonManager != null)
            {
                dungeonManager.EnterNode(dungeonNode);
            }
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
                LogEx.LogWarning($"dungeonNode is null");
                return;
            }
            
            DungeonNodeSettingSO.SpriteSet spriteSet = setting.GetSpriteSet(dungeonNode.Type);
            switch (dungeonNode.State)
            {
                case NodeState.Locked:
                    nodeImage.sprite = spriteSet.Inactive;
                    nodeText.text = "";
                    _button.interactable = false;
                    _button.gameObject.SetActive(false);
                    SetOverlaySprite(null);
                    break;
                case NodeState.Available:
                    nodeImage.sprite = spriteSet.Active;
                    _button.interactable = true;
                    _button.gameObject.SetActive(true);
                    ForceEnableButtonNextFrame().Forget();
                    SetOverlaySprite(null);
                    break;
                case NodeState.Current:
                    nodeImage.sprite = spriteSet.Active;
                    _button.interactable = false;
                    _button.gameObject.SetActive(true);
                    SetOverlaySprite(null);
                    break;
                case NodeState.Completed:
                    nodeImage.sprite = spriteSet.Completed;
                    nodeText.text = "";
                    _button.interactable = false;
                    _button.gameObject.SetActive(true);
                    SetOverlaySprite(spriteSet.CompletedOverlay);
                    break;
            }
        }

        private async UniTaskVoid ForceEnableButtonNextFrame()
        {
            await UniTask.Yield();
            
            if (_button != null && !_button.gameObject.activeSelf && dungeonNode?.State == NodeState.Available)
            {
                _button.gameObject.SetActive(true);
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


