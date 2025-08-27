using Cardevil.Attributes;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Cardevil.Dungeon
{
    [Serializable]
    public class DungeonNode : ISerializationCallbackReceiver
    {
        [SerializeField, VisibleOnly] private Dungeon dungeon;
        [SerializeField, VisibleOnly] private int nodeId;
        [SerializeField, VisibleOnly] private int floor;
        [SerializeField, VisibleOnly] private DungeonNodeTypes type;
        [SerializeField, VisibleOnly] private DungeonNodePreset preset;

        public Dungeon Dungeon
        {
            get => dungeon;
            set => dungeon = value;
        }
        public int NodeId
        {
            get => nodeId;
            set => nodeId = value;
        }
        public int Floor 
        {
            get => floor;
            set => floor = value;
        }
        public DungeonNodeTypes Type 
        {
            get => type;
            set => type = value;
        }
        public DungeonNodePreset Preset 
        {
            get => preset;
            set => preset = value;
        }
        
        [field:SerializeField] public bool IsBranchStart { get; set; } = false;
        [field:SerializeField] public bool IsInBranch { get; set; } = false;
        [field:SerializeField] public bool IsBranchEnd { get; set; } = false;
        public List<DungeonNode> PreviousNodes { get; private set; } = new List<DungeonNode>();
        public List<DungeonNode> NextNodes { get; private set; } = new List<DungeonNode>();

        public DungeonNode(int nodeId, int floor, DungeonNodeTypes type, DungeonNodePreset preset)
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

        public void OnBeforeSerialize()
        {
            
        }

        public void OnAfterDeserialize()
        {
            
        }
    }
}