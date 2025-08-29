
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Cardevil.Dungeon
{
    [Serializable]
    public class Dungeon : INodeContainer
    {
        [SerializeField] internal int dungeonId;
        [SerializeField] internal DungeonConfigurationSO dungeonConfiguration;
        [SerializeField] private DungeonNode rootNode;
        [SerializeField] private List<DungeonNode> nodes;
        
        
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
            foreach (var n in nodes)
            {
                if (n.NodeId == nodeId)
                {
                    return n;
                }
            }
            Debug.LogError($"[Dungeon] Node with ID {nodeId} not found in Dungeon {dungeonId}");
            return null;
        }
        
        public void Initialize()
        {

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
                sb.AppendLine($"{new string(' ', indent)}Node ID: {current.NodeId}, Type: {current.Type}, Floor: {current.Floor}");

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