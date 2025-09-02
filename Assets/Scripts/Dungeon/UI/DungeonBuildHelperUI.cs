using System;
using System.Collections.Generic;


using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace Cardevil.Dungeon.UI
{
    public class DungeonBuildHelperUI : MonoBehaviour
    {
        public DungeonNodeUIDataComponent rootNode;
        public int startNodeId = 1;
        public bool showLineRenderer = true;
        
        [ContextMenu("Reset Node IDs")]
        public void ResetNodeIds()
        {
            if (rootNode == null)
            {
                Debug.LogError("Root node is not assigned.");
                return;
            }

            int currentId = startNodeId;
            HashSet<DungeonNodeUIDataComponent> visited = new HashSet<DungeonNodeUIDataComponent>();
            Queue<DungeonNodeUIDataComponent> queue = new Queue<DungeonNodeUIDataComponent>();
            queue.Enqueue(rootNode);
            visited.Add(rootNode);

            while (queue.Count > 0)
            {
                DungeonNodeUIDataComponent currentNode = queue.Dequeue();
                currentNode.nodeId = currentId++;
                currentNode.OnValidate();
                
                foreach (var nextNode in currentNode.nextNodes)
                {
                    if (!visited.Contains(nextNode))
                    {
                        visited.Add(nextNode);
                        queue.Enqueue(nextNode);
                    }
                }
                #if UNITY_EDITOR
                PrefabUtility.RecordPrefabInstancePropertyModifications(currentNode);
                #endif
            }

            Debug.Log("Node IDs have been reset.");
        }
        
        [ContextMenu("Set Node Texts to Type")]
        public void SetNodeTextsToType()
        {
            foreach (var node in this)
            {
                if (node == null) continue;
                
                if (node.GetComponentInChildren<TMPro.TextMeshProUGUI>() is { } text)
                {
                    
                    #if UNITY_EDITOR
                    text.text = $"{node.nodeType}";
                    PrefabUtility.RecordPrefabInstancePropertyModifications(text);
                    #else
                    text.text = $"{node.nodeType}";
                    #endif
                }
                else
                {
                    Debug.LogWarning($"No TextMeshProUGUI found in children of node ID {node.nodeId}");
                }
            }
            Debug.Log("Node texts have been updated to their types.");
        }

        public Dungeon BuildDungeon()
        {
            Dungeon dungeon = new Dungeon();
            List<DungeonNode> nodes = new List<DungeonNode>();

            /*
             * 1. 노드 생성 및 리스트에 추가
             */
            foreach (var node in this)
            {
                if (node == null) continue;

                DungeonNode dungeonNode = new DungeonNode(
                    node.nodeId,
                    node.nodeFloor,
                    node.nodeType,
                    node.nodePreset
                );
                nodes.Add(dungeonNode);
                node.GetComponent<DungeonNodeUI>().DungeonNode = dungeonNode;
            }

            dungeon.Nodes = nodes;

            /*
             * 2. 노드 간 연결 설정
             */
            foreach (var dataNode in this)
            {
                if (dataNode == null) continue;

                DungeonNode currentDungeonNode = dungeon.GetNode(dataNode.nodeId);

                foreach (var nextDataNode in dataNode.nextNodes)
                {
                    if (nextDataNode == null) continue;

                    DungeonNode nextDungeonNode = dungeon.GetNode(nextDataNode.nodeId);
                    if (nextDungeonNode == null)
                    {
                        Debug.LogError($"No DungeonNode found for next node ID {nextDataNode.nodeId}");
                        continue;
                    }

                    currentDungeonNode.NextNodes.Add(nextDungeonNode);
                }

                foreach (var prevDataNode in dataNode.prevNodes)
                {
                    if (prevDataNode == null) continue;

                    DungeonNode prevDungeonNode = dungeon.GetNode(prevDataNode.nodeId);
                    if (prevDungeonNode == null)
                    {
                        Debug.LogError($"No DungeonNode found for previous node ID {prevDataNode.nodeId}");
                        continue;
                    }

                    currentDungeonNode.PreviousNodes.Add(prevDungeonNode);
                }
            }

            var root = dungeon.GetNode(rootNode.nodeId);
            dungeon.RootNode = root;

            Queue<DungeonNode> queue = new Queue<DungeonNode>();
            bool isInBranch = false;

            void MarkBranchStartAndEnd()
            {
                queue.Enqueue(root);
                while (queue.Count > 0)
                {
                    DungeonNode currentNode = queue.Dequeue();
                    if (currentNode.NextNodes.Count > 1)
                    {
                        currentNode.IsBranchStart = true;
                    }
                    if (currentNode.PreviousNodes.Count > 1)
                    {
                        foreach (var prev in currentNode.PreviousNodes)
                        {
                            prev.IsBranchEnd = true;
                        }
                    }
                    foreach (var next in currentNode.NextNodes)
                    {
                        queue.Enqueue(next);
                    }
                }
            }
            
            void MarkInBranch(DungeonNode node)
            {
                if (node == null) return;
                if (node.IsInBranch) return; // 이미 방문한 노드면 종료
                if (node.IsBranchEnd)
                {
                    node.IsInBranch = true;
                    return; // 브랜치 끝 노드면 종료
                }
                node.IsInBranch = true;
                foreach (var next in node.NextNodes)
                {
                    MarkInBranch(next);
                } 
            }
            
            MarkBranchStartAndEnd();
            foreach (var node in dungeon.Nodes)
            {
                if (node.IsBranchStart)
                {
                    foreach (var next in node.NextNodes)
                    {
                        MarkInBranch(next);
                    }
                }
            }


            
            return dungeon;
        }

        private void OnValidate()
        {
            if (showLineRenderer)
            {
                foreach (var node in this)
                {
                    if (node == null) continue;
                    
                    node.showDebugLines = true;
                    node.OnValidate();
                }
            }
        }
        

        public IEnumerator<DungeonNodeUIDataComponent> GetEnumerator()
        {
            if (rootNode == null)
            {
                yield break;
            }

            HashSet<DungeonNodeUIDataComponent> visited = new HashSet<DungeonNodeUIDataComponent>();
            Queue<DungeonNodeUIDataComponent> queue = new Queue<DungeonNodeUIDataComponent>();
            queue.Enqueue(rootNode);
            visited.Add(rootNode);

            while (queue.Count > 0)
            {
                DungeonNodeUIDataComponent currentNode = queue.Dequeue();
                yield return currentNode;

                foreach (var nextNode in currentNode.nextNodes)
                {
                    if (!visited.Contains(nextNode))
                    {
                        visited.Add(nextNode);
                        queue.Enqueue(nextNode);
                    }
                }
            }
        }
    }
}