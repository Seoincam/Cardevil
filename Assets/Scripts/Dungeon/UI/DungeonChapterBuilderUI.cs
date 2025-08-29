using System;
using System.Collections.Generic;
using UnityEngine;

namespace Cardevil.Dungeon.UI
{
    public class DungeonChapterBuilderUI : MonoBehaviour, INodeContainer
    {
        public DungeonUI dungeonUI;
        public DungeonChapterUI dungeonChapterUI;
        public DungeonNodeUI nodeUiPrefab;
        public DungeonNodeUI rootNodeUi;
        public List<DungeonNodeUI> nodeUis = new List<DungeonNodeUI>();
        public List<DungeonNode> dungeonNodes = new List<DungeonNode>();
        
        public int cursor = 0;

        private void Reset()
        {
            dungeonUI = GetComponentInParent<DungeonUI>();
            dungeonChapterUI = GetComponent<DungeonChapterUI>();
        }

        public void CreateAndConnect()
        {
            if (nodeUiPrefab == null)
            {
                Debug.LogError("Node UI Prefab is not assigned.");
                return;
            }
            DungeonNodeUI nodeUI = Instantiate(nodeUiPrefab, transform);
            nodeUI.InitRef(dungeonUI, dungeonChapterUI);
        }

        public void Build()
        {
            /*
             * 1. DungeonNodeUIDataComponent -> DungeonNodeData
             * 2. DungeonNodeData -> DungeonNode
             * 3. DungeonNode를 Dungeon에 추가
             * 4. DungeonNodeUI와 DungeonNode 연결
             */
            if (dungeonChapterUI == null || dungeonChapterUI.Dungeon == null)
            {
                Debug.LogError("DungeonChapterUI or its Dungeon is not assigned.");
                return;
            }
            Dungeon dungeon = new Dungeon();
            List<DungeonNodeData> nodeDatas = new List<DungeonNodeData>();
            List<DungeonNode> nodes = new List<DungeonNode>();
            foreach (var nodeUI in nodeUis)
            {
                DungeonNodeUIDataComponent dataComponent = nodeUI.GetComponent<DungeonNodeUIDataComponent>();
                if (dataComponent == null)
                {
                    Debug.LogError("Node UI is missing DungeonNodeUIDataComponent.");
                    continue;
                }

                DungeonNodeData nodeData = new DungeonNodeData();
                dataComponent.ApplyToNodeData(nodeData);
                nodeDatas.Add(nodeData);
                DungeonNode node = DungeonNode.CreateVoid();
                nodeData.ApplyDefaultDataTo(node);
                nodes.Add(node);
            }
            
            // Edge 설정
            for(int i = 0; i < nodeDatas.Count; i++)
            {
                nodeDatas[i].ApplyEdgeDataTo(this, nodes[i]);
            }
            
        }

        public DungeonNode GetNode(int nodeId)
        {
            foreach (var node in dungeonNodes)
            {
                if (node.NodeId == nodeId)
                {
                    return node;
                }
            }
            return null;
        }
    }
}