using System;
using System.Collections.Generic;

namespace Cardevil.Dungeon
{
    [Serializable]
    public class DungeonNodeData
    {
        public int NodeId;
        public int Floor;
        public DungeonNodeTypes Type;
        public DungeonNodePreset Preset;
        public bool IsBranchStart = false;
        public bool IsInBranch = false;
        public bool IsBranchEnd = false;
        public List<int> PreviousNodeIds = new List<int>();
        public List<int> NextNodeIds = new List<int>();

        public void ApplyDefaultDataTo(DungeonNode node)
        {
            node.NodeId = NodeId;
            node.Floor = Floor;
            node.Type = Type;
            node.Preset = Preset;
            node.IsBranchStart = IsBranchStart;
            node.IsInBranch = IsInBranch;
            node.IsBranchEnd = IsBranchEnd;
        }
        
        public void ApplyEdgeDataTo(INodeContainer container, DungeonNode node)
        {
            node.PreviousNodes.Clear();
            foreach (var prevId in PreviousNodeIds)
            {
                var prevNode = container.GetNode(prevId);
                if (prevNode != null)
                {
                    node.PreviousNodes.Add(prevNode);
                }
            }
            node.NextNodes.Clear();
            foreach (var nextId in NextNodeIds)
            {
                var nextNode = container.GetNode(nextId);
                if (nextNode != null)
                {
                    node.NextNodes.Add(nextNode);
                }
            }
        }
        
        public void GetFrom(DungeonNode node)
        {
            NodeId = node.NodeId;
            Floor = node.Floor;
            Type = node.Type;
            Preset = node.Preset;
            IsBranchStart = node.IsBranchStart;
            IsInBranch = node.IsInBranch;
            IsBranchEnd = node.IsBranchEnd;
            PreviousNodeIds.Clear();
            foreach (var prevNode in node.PreviousNodes)
            {
                PreviousNodeIds.Add(prevNode.NodeId);
            }
            NextNodeIds.Clear();
            foreach (var nextNode in node.NextNodes)
            {
                NextNodeIds.Add(nextNode.NodeId);
            }
        }
    }
}