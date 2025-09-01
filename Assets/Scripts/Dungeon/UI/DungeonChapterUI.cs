using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace Cardevil.Dungeon.UI
{
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
        

        [ContextMenu("Create Node UI")]
        public void CreateNodeUI()
        {
            if (nodeUiPrefab == null)
            {
                Debug.LogError("Node UI Prefab is not assigned.");
                return;
            }

            DungeonNodeUI newNodeUI = Instantiate(nodeUiPrefab, transform);
            newNodeUI.InitRef(dungeonUI, this);
            newNodeUI.transform.position = nodeUis[^1].transform.position + new Vector3(100, -100, 0);
            nodeUis.Add(newNodeUI);
            newNodeUI.name = $"NodeUI_{nodeUis.Count}";
        }
        
        
        public DungeonNodeUI GetNodeUI(int nodeId)
        {
            foreach (DungeonNodeUI nodeUi in nodeUis)
            {
                if (nodeUi.DungeonId == dungeonId)
                {
                    return nodeUi;
                }
            }
            return null;
        }
    }
}