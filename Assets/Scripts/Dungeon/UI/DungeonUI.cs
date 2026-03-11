using Cardevil.Core.Attributes;
using Cardevil.Core.Utils;
using Cardevil.DebugConsole;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Threading;
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
        [SerializeField] private Canvas _dungeonUICanvas = null;
        [SerializeField] private RectTransform _rectTransform = null;
        [SerializeField] private List<DungeonChapterUI> _dungeonChapters = new List<DungeonChapterUI>();
        [SerializeField] DungeonUICamera _dungeonUICamera = null;
        [SerializeField] private Ease _moveEase = Ease.InOutSine;
        
        [SerializeField,VisibleOnly] private int currentDungeonId = -1;
        private DungeonChapterUI CurrentDungeonUI => _dungeonChapters.Find(chapter => chapter.DungeonId == currentDungeonId);
        
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
            
            foreach (DungeonChapterUI chapterUI in _dungeonChapters)
            {
                chapterUI.gameObject.SetActive(false);
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
            
            if (fromUI != null)
            {
                fromUI.gameObject.SetActive(false);
            }
            toShow.gameObject.SetActive(true);
        }
        
        
        private float GetPosY(int currentChapterIndex)
        {
            float posY = 0f + currentChapterIndex * Height;
            return posY;
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