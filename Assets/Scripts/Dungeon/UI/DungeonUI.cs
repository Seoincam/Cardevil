using Cardevil.Attributes;
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
        [SerializeField] private List<DungeonChapterUI> _dungeonChapters = new List<DungeonChapterUI>();
        [SerializeField] DungeonUICamera _dungeonUICamera = null;
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

        private void Reset()
        {
            _dungeonUICanvas = GetComponentInParent<Canvas>();      
        }

        private void Awake()
        {
            
        }

        public void Initialize()
        {
            LogEx.Log("[DungeonUI] Phase 1 - Setting up UI references...");
            /*
             * 던전 UI 초기화 - 1단계: 참조 설정
             */
            foreach (DungeonChapterUI chapterUI in _dungeonChapters)
            {
                chapterUI.Initialize(this);
            }
        }
        
        /// <summary>
        /// 던전 생성 후 호출되어야 하는 2단계 초기화
        /// </summary>
        public void InitializeAfterDungeonCreated()
        {
            LogEx.Log("[DungeonUI] Phase 2 - Initializing with dungeon data...");
            /*
             * 던전 UI 초기화 - 2단계: 던전 데이터 기반 초기화
             */
            foreach (DungeonChapterUI chapterUI in _dungeonChapters)
            {
                chapterUI.InitializeAfterDungeonCreated();
            }
        }
        
        public void UpdateShowingDungeon(int id)
        {
            var toShow = _dungeonChapters.Find(chapter => chapter.DungeonId == id);
            if (toShow == null)
            {
                LogEx.LogError($"[DungeonUI] No DungeonChapterUI found for dungeon ID {id}");
                return;
            }
            
            foreach (DungeonChapterUI chapterUI in _dungeonChapters)
            {
                chapterUI.gameObject.SetActive(chapterUI == toShow);
            }
            
            Camera.MoveTo(toShow.transform.position).OnComplete(() =>
            {
                foreach (DungeonChapterUI chapterUI in _dungeonChapters)
                {
                    chapterUI.gameObject.SetActive(chapterUI == toShow);
                }
            });
        }




        public void UpdateAll()
        {
            
        }
    }
}