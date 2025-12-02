using Cardevil.Attributes;
using Cardevil.Dungeon;
using Cardevil.Dungeon.UI;
using Cardevil.Utils;
using Cysharp.Threading.Tasks;
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
                    // Awake()가 실행되기 전에 UpdateView()가 호출될 수 있으므로
                    // Start()에서 호출하도록 플래그만 설정
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
                Debug.LogWarning($"DungeonNodeUI InitializeLine: dungeonNode is null on {name}");
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
            // Unity Inspector에서 버튼이 비활성화되어 있을 수 있으므로 강제 활성화
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
            // OnEnable에서도 버튼 강제 활성화 (Unity Scene 복원 대응)
            if (_button != null && !_button.gameObject.activeSelf)
            {
                Debug.Log($"OnEnable: Button was inactive, forcing activation for {name}");
                _button.gameObject.SetActive(true);
            }
        }

        private void Start()
        {
            // Awake() 이후 확실히 버튼이 준비된 상태에서 UpdateView 호출
            if (dungeonNode != null)
            {
                Debug.Log($"Start: Node {dungeonNode.NodeId} - 초기 UpdateView 호출, Button.activeSelf={_button?.gameObject.activeSelf}");
                UpdateView();
            }
            else
            {
                Debug.LogWarning($"Start: dungeonNode is null on {name}");
            }
        }

        public void OnClickButton()
        {
            if (dungeonNode == null)
            {
                Debug.LogWarning($"DungeonNodeUI OnClickButton: dungeonNode is null on {name}");
                return;
            }
            // 들어갈 수 있는 경우에만
            if (dungeonNode.State != NodeState.Available)
            {
                Debug.LogWarning($"DungeonNodeUI OnClickButton: Cannot enter node {dungeonNode.NodeId} - state is {dungeonNode.State}");
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
            Debug.Log($"Node {dungeonNode?.NodeId} OnStateChanged: {newState}, DungeonUI={name}, GameObject.activeInHierarchy={gameObject.activeInHierarchy}, Parent active={transform.parent?.gameObject.activeInHierarchy}");
            UpdateView();
        }

        public void UpdateView()
        {
            if (dungeonNode == null)
            {
                Debug.LogWarning($"UpdateView: dungeonNode is null on {name}");
                return;
            }
            
            Debug.Log($"UpdateView START - Node {dungeonNode.NodeId}, State={dungeonNode.State}, GameObject={name}, active={gameObject.activeInHierarchy}, Button before={_button.gameObject.activeSelf}");
            
            DungeonNodeSettingSO.SpriteSet spriteSet = setting.GetSpriteSet(dungeonNode.Type);
            switch (dungeonNode.State)
            {
                case NodeState.Locked:
                    nodeImage.sprite = spriteSet.Inactive;
                    nodeText.text = "";
                    _button.interactable = false;
                    _button.gameObject.SetActive(false);
                    Debug.Log($"UpdateView END - Node {dungeonNode.NodeId} LOCKED: Button.activeSelf={_button.gameObject.activeSelf}, Button.activeInHierarchy={_button.gameObject.activeInHierarchy}");
                    SetOverlaySprite(null);
                    break;
                case NodeState.Available:
                    nodeImage.sprite = spriteSet.Active;
                    _button.interactable = true;
                    _button.gameObject.SetActive(true);
                    
                    // 다음 프레임에도 한 번 더 확인 (Unity Scene 복원 대응)
                    ForceEnableButtonNextFrame().Forget();
                    
                    Debug.Log($"UpdateView END - Node {dungeonNode.NodeId} AVAILABLE: Button.activeSelf={_button.gameObject.activeSelf}, Button.activeInHierarchy={_button.gameObject.activeInHierarchy}");
                    SetOverlaySprite(null);
                    break;
                case NodeState.Current:
                    nodeImage.sprite = spriteSet.Active;
                    _button.interactable = false;
                    _button.gameObject.SetActive(true);
                    Debug.Log($"UpdateView END - Node {dungeonNode.NodeId} CURRENT: Button.activeSelf={_button.gameObject.activeSelf}, Button.activeInHierarchy={_button.gameObject.activeInHierarchy}");
                    SetOverlaySprite(null);
                    break;
                case NodeState.Completed:
                    nodeImage.sprite = spriteSet.Completed;
                    nodeText.text = "";
                    _button.interactable = false;
                    _button.gameObject.SetActive(true);
                    Debug.Log($"UpdateView END - Node {dungeonNode.NodeId} COMPLETED: Button.activeSelf={_button.gameObject.activeSelf}, Button.activeInHierarchy={_button.gameObject.activeInHierarchy}");
                    SetOverlaySprite(spriteSet.CompletedOverlay);
                    break;
            }
        }

        private async UniTaskVoid ForceEnableButtonNextFrame()
        {
            await UniTask.Yield();
            
            if (_button != null && !_button.gameObject.activeSelf && dungeonNode?.State == NodeState.Available)
            {
                Debug.Log($"ForceEnableButtonNextFrame: 다음 프레임에서 버튼 재활성화 - Node {dungeonNode.NodeId}");
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