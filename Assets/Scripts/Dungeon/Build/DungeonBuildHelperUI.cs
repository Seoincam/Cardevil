using Cardevil.Dungeon.UI;
using Cardevil.Utils;
using System;
using System.Collections.Generic;


using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace Cardevil.Dungeon.Build
{
    /// <summary>
    /// Unity Editor에서 UI 기반으로 던전을 빌드하는 헬퍼 클래스
    /// BFS 알고리즘을 사용하여 노드를 순회하고, Branch를 마킹합니다.
    /// </summary>
    public class DungeonBuildHelperUI : MonoBehaviour
    {
        public DungeonNodeUIDataComponent rootNode;
        public int startNodeId = 1;
        [SerializeField] private ContainerNode nodeContainer;
        
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
        
        [ContextMenu("Clear All Connections")]
        public void ClearAllConnections()
        {
            foreach (var node in this)
            {
                if (node == null) continue;
                
                node.nextNodes.Clear();
                node.prevNodes.Clear();
                node.OnValidate();
                #if UNITY_EDITOR
                if(Application.isPlaying) continue;
                PrefabUtility.RecordPrefabInstancePropertyModifications(node);
                #endif
            }
            Debug.Log("All node connections have been cleared.");
        }
        /// <summary>
        /// 계층 구조를 기반으로 모든 노드들을 연결합니다.
        /// 고맙다 제미나이야
        /// </summary>
        [ContextMenu("Connect All Nodes By Hierarchy")]
        public void ReconnectAllNodesByHierarchy()
        {
            // 이전과 동일한 노드 연결 헬퍼 함수
            void Connect(DungeonNodeUIDataComponent prev, DungeonNodeUIDataComponent next)
            {
                if (prev == null || next == null) return;

                if (!prev.nextNodes.Contains(next))
                {
                    prev.nextNodes.Add(next);
                }

                if (!next.prevNodes.Contains(prev))
                {
                    next.prevNodes.Add(prev);
                }
            }

    /*
     * 재귀적으로 계층 구조를 탐색하여 노드를 연결합니다.
     * containerNode 탐색할 부모 컨테이너 노드
     * previousNodes 이 컨테이너의 첫 노드와 연결될 이전 노드들의 리스트
     * return 현재 컨테이너 탐색을 마친 후의 '마지막 노드'들의 리스트. 분기가 있으면 여러 개일 수 있습니다.
     */
    List<DungeonNodeUIDataComponent> StartConnecting(ContainerNode containerNode, List<DungeonNodeUIDataComponent> previousNodes)
    {
        // 현재 스코프에서 '마지막 노드'로 간주되는 노드들의 리스트.
        // 처음에는 부모로부터 전달받은 previousNodes로 초기화합니다.
        var currentLastNodes = new List<DungeonNodeUIDataComponent>(previousNodes);

        // 컨테이너의 모든 자식 오브젝트를 순서대로 순회합니다.
        for (int i = 0; i < containerNode.transform.childCount; i++)
        {
            var child = containerNode.transform.GetChild(i);
            if (child == null) continue;

            // 1. 자식이 실제 '노드'일 경우
            if (child.TryGetComponent<DungeonNodeUIDataComponent>(out var node))
            {
                // 이전 노드들(currentLastNodes)을 현재 노드(node)에 모두 연결합니다.
                foreach (var prev in currentLastNodes)
                {
                    Connect(prev, node);
                }
                // 연결이 끝났으므로, 이제 '마지막 노드'는 현재 노드 하나뿐입니다. 리스트를 갱신합니다.
                currentLastNodes = new List<DungeonNodeUIDataComponent> { node };
            }
            // 2. 자식이 또 다른 '컨테이너'일 경우
            else if (child.TryGetComponent<ContainerNode>(out var childContainer))
            {
                // 2-1. 컨테이너가 '분기점(Branch Point)'일 경우
                if (childContainer.isBranchPoint)
                {
                    // 모든 하위 분기들이 끝난 후, 각 분기의 마지막 노드들을 수집할 리스트입니다.
                    var allBranchEndNodes = new List<DungeonNodeUIDataComponent>();

                    // 분기점 컨테이너의 자식들(각각의 분기)을 순회합니다.
                    for (int j = 0; j < childContainer.transform.childCount; j++)
                    {
                        var branch = childContainer.transform.GetChild(j);
                        if (branch.TryGetComponent<ContainerNode>(out var branchContainer) && branchContainer.isBranchChild)
                        {
                            // 각 분기는 '분기점 직전의 노드(currentLastNodes)'에서 시작합니다.
                            // 재귀 호출을 통해 해당 분기의 마지막 노드들을 받아옵니다.
                            var branchEndNodes = StartConnecting(branchContainer, currentLastNodes);
                            // 반환된 마지막 노드들을 전체 분기 마지막 노드 리스트에 추가합니다.
                            allBranchEndNodes.AddRange(branchEndNodes);
                        }
                    }
                    
                    // 모든 분기 탐색이 끝나면, '마지막 노드'들은 모든 분기들의 마지막 노드들이 됩니다.
                    currentLastNodes = allBranchEndNodes;
                }
                // 2-2. 일반 컨테이너일 경우 (isBranchChild는 isBranchPoint에 의해 처리되므로 별도 로직 불필요)
                else
                {
                    // 현재까지의 마지막 노드(currentLastNodes)를 그대로 넘겨주어 연결을 이어갑니다.
                    // 재귀 호출의 결과(하위 컨테이너의 마지막 노드들)로 currentLastNodes를 갱신합니다.
                    currentLastNodes = StartConnecting(childContainer, currentLastNodes);
                }
            }
        }
        
        // 이 컨테이너의 탐색이 모두 끝난 시점의 마지막 노드 리스트를 반환합니다.
        return currentLastNodes;
    }
    
    // 최상위 노드 컨테이너에서부터 빈 '이전 노드 리스트'와 함께 연결을 시작합니다.
    StartConnecting(nodeContainer, new List<DungeonNodeUIDataComponent>());

    // 모든 노드에 변경사항을 적용합니다.
    foreach (var node in this)
    {
        if (node == null) continue;
        
        node.OnValidate();
        #if UNITY_EDITOR
        if(Application.isPlaying) continue;
        UnityEditor.PrefabUtility.RecordPrefabInstancePropertyModifications(node);
        #endif
    }
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
            
            // DungeonId 설정
            var chapterUI = GetComponent<DungeonChapterUI>();
            if (chapterUI != null)
            {
                dungeon.dungeonId = chapterUI.DungeonId;
            }
            else
            {
                Debug.LogWarning("[DungeonBuildHelper] No DungeonChapterUI found. DungeonId will be -1.");
                dungeon.dungeonId = -1;
            }
            
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
                    nextDungeonNode.Floor = Math.Max(currentDungeonNode.Floor + 1, nextDungeonNode.Floor);
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

            /*
             * 3. Branch 마킹
             */
            Queue<DungeonNode> queue = new Queue<DungeonNode>();
            HashSet<DungeonNode> visitedForMarking = new HashSet<DungeonNode>();

            void MarkBranchStartAndEnd()
            {
                queue.Enqueue(root);
                visitedForMarking.Add(root);
                
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
                        if (!visitedForMarking.Contains(next))
                        {
                            visitedForMarking.Add(next);
                            queue.Enqueue(next);
                        }
                    }
                }
            }
            
            void MarkInBranch(DungeonNode node, HashSet<DungeonNode> visited)
            {
                if (node == null) return;
                if (visited.Contains(node)) return; // 순환 참조 방지
                if (node.IsInBranch) return; // 이미 방문한 노드면 종료
                
                visited.Add(node);
                
                if (node.IsBranchEnd)
                {
                    node.IsInBranch = true;
                    return; // 브랜치 끝 노드면 종료
                }
                node.IsInBranch = true;
                foreach (var next in node.NextNodes)
                {
                    MarkInBranch(next, visited);
                } 
            }
            
            MarkBranchStartAndEnd();
            foreach (var node in dungeon.Nodes)
            {
                if (node.IsBranchStart)
                {
                    foreach (var next in node.NextNodes)
                    {
                        HashSet<DungeonNode> branchVisited = new HashSet<DungeonNode>();
                        MarkInBranch(next, branchVisited);
                    }
                }
            }

            /*
             * 4. 던전 초기화
             */
            dungeon.Initialize();
            
            return dungeon;
        }
        

        public IEnumerator<DungeonNodeUIDataComponent> GetEnumerator()
        {
            if (nodeContainer == null)
            {
                yield break;
            }
            var allNodes = nodeContainer.GetComponentsInChildren<DungeonNodeUIDataComponent>();
            foreach (var node in allNodes)
            {
                yield return node;
            }
        }
    }
}
