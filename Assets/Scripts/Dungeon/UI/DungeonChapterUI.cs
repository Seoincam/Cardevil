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
            this.dungeonUI = dungeonUI;
            
            var allChildren = GetComponentsInChildren<DungeonNodeUI>(true);
            nodeUis = new List<DungeonNodeUI>(allChildren);
            
            foreach (DungeonNodeUI nodeUi in nodeUis)
            {
                nodeUi.InitRef(dungeonUI, this);
            }
        }

        public void InitializeAfterDungeonCreated()
        {
            Dungeon dungeon = Managers.Dungeon.GetDungeonById(dungeonId);
            if (dungeon == null)
            {
                LogEx.LogError($"Dungeon with ID {dungeonId} not found");
                return;
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