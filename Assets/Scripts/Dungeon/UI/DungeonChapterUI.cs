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
        public Dungeon Dungeon => Managers.Dungeon.GetDungeonById(dungeonId);


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
            LogEx.Log($"[DungeonChapterUI] Phase 1 - Setting up references for Dungeon ID: {dungeonId}");
            this.dungeonUI = dungeonUI;
            
            // 노드 UI 받아오기
            var allChildren = GetComponentsInChildren<DungeonNodeUI>(true);
            nodeUis = new List<DungeonNodeUI>(allChildren);
            
            // 모든 노드에 참조만 설정 (던전 데이터는 아직 없음)
            foreach (DungeonNodeUI nodeUi in nodeUis)
            {
                nodeUi.InitRef(dungeonUI, this);
            }
        }

        /// <summary>
        /// 던전이 생성된 후 호출되어야 하는 2단계 초기화
        /// </summary>
        public void InitializeAfterDungeonCreated()
        {
            LogEx.Log($"[DungeonChapterUI] Phase 2 - Initializing with dungeon data for Dungeon ID: {dungeonId}");
            
            Dungeon dungeon = Managers.Dungeon.GetDungeonById(dungeonId);
            if (dungeon == null)
            {
                LogEx.LogError($"Dungeon with ID {dungeonId} not found.");
                return;
            }

            // 라인 초기화 (던전 노드 데이터 필요)
            foreach (DungeonNodeUI nodeUi in nodeUis)
            {
                nodeUi.InitializeLine();
            }
        }

        public DungeonNodeUI GetNodeUI(int nodeId)
        {
            foreach (DungeonNodeUI nodeUi in nodeUis)
            {
                if (nodeUi.DungeonNode != null && nodeUi.DungeonNode.NodeId == nodeId)
                {
                    return nodeUi;
                }
            }
            Debug.LogWarning($"No DungeonNodeUI found for node ID {nodeId} in dungeon {dungeonId}");
            return null;
        }
    }
}