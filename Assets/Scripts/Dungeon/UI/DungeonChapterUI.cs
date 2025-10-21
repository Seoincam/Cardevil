using Cardevil.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace Cardevil.Dungeon.UI
{
    /// <summary>
    /// 던전 챕터 UI 클래스
    /// </summary>
    public class DungeonChapterUI : MonoBehaviour
    {
        [SerializeField] private int dungeonId = -1;
        [Header("References")]
        [SerializeField] private DungeonUI dungeonUI;
        [SerializeField] private DungeonNodeUI nodeUiPrefab;
        [SerializeField] private List<DungeonNodeUI> nodeUis = new List<DungeonNodeUI>();

        [SerializeField] private int cursor = 0;
        public int DungeonId => dungeonId;
        public Dungeon Dungeon => Managers.Dungeon.GetDungeon(dungeonId);


        private void Awake()
        {
            if (dungeonUI == null)
            {
                dungeonUI = GetComponentInParent<DungeonUI>();
                if (dungeonUI == null)
                {
                    Debug.LogError("DungeonUI reference is not assigned and not found in parent.");
                }
            }
        }

        public void Initialize(DungeonUI dungeonUI)
        {
            LogEx.Log($"Initializing DungeonChapterUI for Dungeon ID: {dungeonId}");
            this.dungeonUI = dungeonUI;
            Dungeon dungeon = Managers.Dungeon.GetDungeon(dungeonId);
            if (dungeon == null)
            {
                LogEx.LogError($"Dungeon with ID {dungeonId} not found.");
                return;
            }
            
            //노드 UI 받아오기
            var allChildren = GetComponentsInChildren<DungeonNodeUI>(true);
            nodeUis = new List<DungeonNodeUI>(allChildren);
            

            //모든 노드 초기화
            foreach (DungeonNodeUI nodeUi in nodeUis)
            {
                nodeUi.InitRef(dungeonUI, this);
            }

            foreach (DungeonNodeUI nodeUi in nodeUis)
            {
                nodeUi.InitializeLine();
            }
        }
        
        public DungeonNodeUI GetNodeUI(int nodeId)
        {
            foreach (DungeonNodeUI nodeUi in nodeUis)
            {
                if (nodeUi.DungeonId == nodeId)
                {
                    return nodeUi;
                }
            }
            return null;
        }
    }
}