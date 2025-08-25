using System;
using System.Collections.Generic;
using UnityEngine;

namespace Cardevil.Dungeon
{
    [Serializable]
    public class DungeonNode
    {
        public int NodeId { get; set; }
        public int Floor { get; set; }
        public DungeonNodeTypes Type { get; private set; }
        public DungeonNodePreset Preset { get; private set; }
        
        public bool IsBranchStart { get; set; } = false;
        public bool IsInBranch { get; set; } = false;
        public bool IsBranchEnd { get; set; } = false;
        public List<DungeonNode> PreviousNodes { get; private set; } = new List<DungeonNode>();
        public List<DungeonNode> NextNodes { get; private set; } = new List<DungeonNode>();

        public DungeonNode(int nodeId,int floor, DungeonNodeTypes type, DungeonNodePreset preset)
        {
            NodeId = nodeId;
            Floor = floor;
            Type = type;
            Preset = preset;
        }

        /// <summary>
        /// 던전 빌드 후에 실행
        /// </summary>
        public void Initialize()
        {
            // Initialization logic for the node can be added here
        }
        
        public void LinkTo(DungeonNode nextNode)
        {
            if (nextNode == null) return;
            NextNodes.Add(nextNode);
            nextNode.PreviousNodes.Add(this);
        }
        
        public void LinkToFirst(DungeonNode firstNode)
        {
            if (firstNode == null) return;
            NextNodes.Insert(0, firstNode);
            firstNode.PreviousNodes.Add(this);
        }
        
        public override string ToString()
        {
            if (Preset != null)
            {
                return $"NodeId: {NodeId}, Floor: {Floor}, Type: {Type}, Preset: {Preset.GetType().Name}";
            }
            else
            {
                return $"NodeId: {NodeId}, Floor: {Floor}, Type: {Type}, Preset: None";
            }
        }
    }
}