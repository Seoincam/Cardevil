using Cardevil.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Cardevil.Dungeon
{
    
    /// <summary>
    /// 던전 데이터 클래스.
    /// 일종의 Node 유향 그래프.
    /// </summary>
    [Serializable]
    public class Dungeon : INodeContainer
    {
        [SerializeField] internal int dungeonId;
        [SerializeField] internal DungeonConfigurationSO dungeonConfiguration;
        [SerializeField] private DungeonNode rootNode;
        [SerializeField] private List<DungeonNode> nodes;
        [NonSerialized] private Dictionary<int, DungeonNode> nodeCache;
        
        public int DungeonId
        {
            get => dungeonId;
        }
        public DungeonConfigurationSO DungeonConfiguration
        {
            get => dungeonConfiguration;
        }
        
        public DungeonNode RootNode
        {
            get => rootNode;
            set => rootNode = value;
        }
        public List<DungeonNode> Nodes
        {
            get => nodes;
            set => nodes = value;
        }
        
        public DungeonNode GetNode(int nodeId)
        {
            // 캐시가 있으면 사용
            if (nodeCache != null && nodeCache.TryGetValue(nodeId, out var cachedNode))
            {
                return cachedNode;
            }
            
            // 캐시가 없으면 선형 탐색 (Fallback)
            foreach (var n in nodes)
            {
                if (n.NodeId == nodeId)
                {
                    return n;
                }
            }
            LogEx.LogError($"[Dungeon] Node with ID {nodeId} not found in Dungeon {dungeonId}");
            return null;
        }
        
        public void Initialize()
        {
            if (nodes == null || nodes.Count == 0)
            {
                LogEx.LogWarning($"[Dungeon] Initializing empty dungeon {dungeonId}");
                return;
            }
            
            // 노드 캐시 생성
            nodeCache = new Dictionary<int, DungeonNode>();
            foreach (var node in nodes)
            {
                if (node == null) continue;
                
                nodeCache[node.NodeId] = node;
                node.Dungeon = this;
                node.Initialize();
            }
            
            LogEx.Log($"[Dungeon] Initialized dungeon {dungeonId} with {nodes.Count} nodes");
        }

        public string GetDebugString()
        {
            if (nodes == null || nodes.Count == 0)
            {
                return "== Empty Dungeon ==";
            }
        
            StringBuilder sb = new StringBuilder();
            Stack<DungeonNode> stack = new Stack<DungeonNode>();
            stack.Push(rootNode);
            sb.AppendLine($"Dungeon ID: {dungeonId}");
            sb.AppendLine("Dungeon Structure:");
            int indent = 0;
            while (stack.Count > 0)
            {
                DungeonNode current = stack.Pop();
                indent = current.IsInBranch ? 4 : 0;
                if (current.IsBranchStart)
                {
                    sb.Append(new string(' ', indent));
                    sb.AppendLine("-- Branch Start --");
                }
                sb.Append($"{new string(' ', indent)}Node ID: {current.NodeId}, Type: {current.Type}, Floor: {current.Floor}");
                if(current.NextNodes.Count > 1)
                {
                    sb.Append(" Next Nodes: ");
                    foreach (var next in current.NextNodes)
                    {
                        sb.Append($"{next.NodeId}, ");
                    }
                    sb.Remove(sb.Length - 2, 2); // 마지막 쉼표와 공백 제거
                }
                sb.AppendLine();

                if (current.IsBranchStart)
                {
                    for(int i = current.NextNodes.Count - 1; i >= 0; i--)
                    {
                        stack.Push(current.NextNodes[i]);
                    }
                }
                else if (current.IsBranchEnd)
                {
                    if (stack.Count == 0)
                    {
                        stack.Push(current.NextNodes[0]);
                    }
                    else
                    {
                        // 아무것도 하지 않기.
                    }
                }
                else
                {
                    foreach (var nextNode in current.NextNodes)
                    {
                        stack.Push(nextNode);
                    }
                }
            }
            sb.AppendLine("End of Dungeon Structure.");
            return sb.ToString();
        }
        
    }
}