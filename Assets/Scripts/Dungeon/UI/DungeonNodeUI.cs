using Cardevil.Attributes;
using Cardevil.Core.Root;
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
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image nodeImage;
        [SerializeField] private TextMeshProUGUI nodeText;
        [FormerlySerializedAs("hoverImage")] [SerializeField] private Image overlayImage;
        [Space]
        [Header("External References")]
        [SerializeField] private DungeonUI dungeonUI;
        [SerializeField] private DungeonChapterUI dungeonChapterUI;
        [Header("Dungeon Node Info")]
        [SerializeReference,VisibleOnly] private DungeonNode dungeonNode;
        [Space]
        [Header("Variables")]
        [SerializeField] private int nodeId = -1;
        

        private LineRenderer lineRenderer;
        
        // UI 컴포넌트 접근용 프로퍼티
        public Button NodeButton => _button;
        public Image BackgroundImage => backgroundImage;
        public Image NodeImage => nodeImage;
        public TextMeshProUGUI NodeText => nodeText;
        public Image OverlayImage => overlayImage;
        public Animator NodeAnimator => nodeAnimator;
        
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
            
            var dungeonManager = WorldRoot.Instance.Dungeon;
            if (dungeonManager != null)
            {
                dungeonManager.EnterNode(dungeonNode);
            }
        }

        private void OnValidate()
        {
        }
        
        private void OnDungeonNodeStateChanged(NodeState newState)
        {
            UpdateView();
        }
        
        public void Hide()
        {
            if (_button != null)
            {
                _button.gameObject.SetActive(false);
            }

            if (overlayImage != null)
            {
                overlayImage.gameObject.SetActive(false);
            }
        }
        
        /// <summary>
        /// 숨겨진 노드를 다시 표시합니다.
        /// </summary>
        public void Show()
        {
            if (_button != null)
            {
                _button.gameObject.SetActive(true);
            }
            
        }

        /// <summary>
        /// 노드 UI를 업데이트합니다. Preset이 직접 UI를 그립니다.
        /// </summary>
        public void UpdateView()
        {
            if (dungeonNode == null)
            {
                LogEx.LogWarning($"dungeonNode is null");
                return;
            }
            
            // Preset이 있으면 Preset이 직접 UI를 그림
            if (dungeonNode.Preset != null)
            {
                dungeonNode.Preset.DrawNodeUI(this, dungeonNode.State);
            }
            else
            {
                // Preset이 없으면 기본 처리
                LogEx.LogWarning($"Node {dungeonNode.NodeId}에 Preset이 없습니다.");
            }
            
            // 상태에 따른 버튼 활성화/비활성화 처리
            UpdateButtonState(dungeonNode.State);
        }
        
        /// <summary>
        /// 상태에 따라 버튼 활성화/비활성화를 처리합니다.
        /// </summary>
        private void UpdateButtonState(NodeState state)
        {
            if (_button == null) return;
            
            switch (state)
            {
                case NodeState.Locked:
                    _button.interactable = false;
                    // _button.gameObject.SetActive(false);
                    break;
                case NodeState.Available:
                    _button.interactable = true;
                    // _button.gameObject.SetActive(true);
                    break;
                case NodeState.Current:
                    _button.interactable = false;
                    // _button.gameObject.SetActive(true);
                    break;
                case NodeState.Completed:
                    _button.interactable = false;
                    // _button.gameObject.SetActive(true);
                    break;
                case NodeState.Passed:
                    _button.interactable = false;
                    // _button.gameObject.SetActive(false);
                    break;
                case NodeState.Hidden:
                    _button.interactable = false;
                    // _button.gameObject.SetActive(false);
                    break;
            }
        }
        
        
        [ContextMenu("Print Node Info")]
        public void PrintNodeInfo()
        {
            Debug.Log(ToString());
            Debug.Log("Previous Nodes:");
            foreach (var prev in dungeonNode.PreviousNodes)
            {
                Debug.Log($"  - NodeId: {prev.NodeId}");
            }
            Debug.Log("Next Nodes:");
            foreach (var next in dungeonNode.NextNodes)
            {
                Debug.Log($"  - NodeId: {next.NodeId}");
            }
        }
        
    }
}


