using Cardevil.Attributes;
using Cardevil.DebugConsole;
using Cardevil.Utils;
using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Cardevil.Dungeon.UI
{
    /// <summary>
    /// 던전 UI 클래스. 
    /// 현재로선 UI Manager를 통하지 않음.(생성/파괴가 없으므로)
    /// </summary>
    public class DungeonUI : MonoBehaviour
    {
        [SerializeField, VisibleOnly] private Canvas _dungeonUICanvas = null;
        [SerializeField, VisibleOnly] private RectTransform _rectTransform = null;
        [SerializeField] private List<DungeonChapterUI> _dungeonChapters = new List<DungeonChapterUI>();
        [SerializeField] DungeonUICamera _dungeonUICamera = null;
        [SerializeField] private Ease _moveEase = Ease.InOutSine;
        
        [SerializeField,VisibleOnly] private int currentDungeonId = -1;
        
        public Canvas Canvas
        {
            get
            {
                if (_dungeonUICanvas == null)
                {
                    _dungeonUICanvas = GetComponentInParent<Canvas>();
                    if (_dungeonUICanvas == null)
                    {
                        LogEx.LogError("DungeonUI: No Canvas component found on the GameObject.");
                    }
                }
                return _dungeonUICanvas;
            }
        }
        
        public DungeonUICamera Camera
        {
            get
            {
                if (_dungeonUICamera == null)
                {
                    _dungeonUICamera = Object.FindAnyObjectByType<DungeonUICamera>();
                }
                return _dungeonUICamera;
            }
        }
        
        public float Width => _rectTransform.rect.width;
        public float Height => _rectTransform.rect.height;

        private void Reset()
        {
            _dungeonUICanvas = GetComponentInParent<Canvas>();      
            _rectTransform = GetComponent<RectTransform>();
        }

        private void Awake()
        {
            if (_dungeonUICanvas == null)
            {
                _dungeonUICanvas = GetComponentInParent<Canvas>();
            }
            if (_rectTransform == null)
            {
                _rectTransform = GetComponent<RectTransform>();
            }
        }

        public void Initialize()
        {
            foreach (DungeonChapterUI chapterUI in _dungeonChapters)
            {
                chapterUI.Initialize(this);
            }

            currentDungeonId = 1;
        }
        
        public void InitializeAfterDungeonCreated()
        {
            foreach (DungeonChapterUI chapterUI in _dungeonChapters)
            {
                chapterUI.InitializeAfterDungeonCreated();
            }
        }
        
        public void UpdateShowingDungeon(int id)
        {
            var fromUI = _dungeonChapters.Find(chapter => chapter.DungeonId == currentDungeonId);
            var toShow = _dungeonChapters.Find(chapter => chapter.DungeonId == id);
            if (toShow == null)
            {
                LogEx.LogError($"No DungeonChapterUI found for dungeon ID {id}");
                return;
            }
            currentDungeonId = id;
            
            //현재와 다음 던전을 활성화
            fromUI.gameObject.SetActive(true);
            toShow.gameObject.SetActive(true);
            
            // Camera.MoveTo(toShow.transform.position).OnComplete(() =>
            // {
            //     foreach (DungeonChapterUI chapterUI in _dungeonChapters)
            //     {
            //         chapterUI.gameObject.SetActive(chapterUI == toShow);
            //     }
            // });
            
            // 카메라가 아니라 던전 UI 자체를 이동시키는 방식으로 변경
            float posY = GetPosY(_dungeonChapters.IndexOf(toShow));
            _rectTransform.DOKill();
            _rectTransform.DOAnchorPosY(posY, 0.5f).SetEase(DG.Tweening.Ease.InOutSine).OnComplete(() =>
            {
                foreach (DungeonChapterUI chapterUI in _dungeonChapters)
                {
                    chapterUI.gameObject.SetActive(chapterUI == toShow);
                }
            });
        }
        
        private float GetPosY(int currentChapterIndex)
        {
            float posY = 0f + currentChapterIndex * Height;
            return posY;
        }

        /// <summary>
        /// 현재 노드의 다음 노드들 중 블랙마켓의 가시성을 업데이트합니다.
        /// 블랙마켓이 나타나지 않으면 해당 노드 UI를 숨깁니다.
        /// </summary>
        /// <param name="currentNode">현재 노드</param>
        public void UpdateBlackMarketVisibility(DungeonNode currentNode)
        {
            if (currentNode == null) return;
            
            var chapterUI = _dungeonChapters.Find(c => c.DungeonId == currentDungeonId);
            if (chapterUI == null) return;
            
            foreach (var nextNode in currentNode.NextNodes)
            {
                if (nextNode.Type == DungeonNodeTypes.BlackMarket && nextNode.IsBlackMarketHidden)
                {
                    var nodeUI = chapterUI.GetNodeUI(nextNode.NodeId);
                    if (nodeUI != null)
                    {
                        nodeUI.HideAsBlackMarketNotAppeared();
                        LogEx.Log($"[DungeonUI] 블랙마켓 노드 UI {nextNode.NodeId} 숨김");
                    }
                }
            }
        }

        public void UpdateAll()
        {
            
        }

        [ConsoleCommand("dungeonUI.ShowDungeon", "Show a specific dungeon UI by its ID.", "dungeonUI.ShowDungeon <dungeonId>", new []{"1", "2", "3"})]
        private static void Console_ShowDungeon(int dungeonId)
        {
            var dungeonUI = Object.FindAnyObjectByType<DungeonUI>();
            if (dungeonUI == null)
            {
                LogEx.LogError("No DungeonUI instance found in the scene.");
                return;
            }
            dungeonUI.UpdateShowingDungeon(dungeonId);
        }
    }
}