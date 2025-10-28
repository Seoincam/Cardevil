using Cardevil.Attributes;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Cardevil.Dungeon
{
    [Serializable]
    public class DungeonNode : ISerializationCallbackReceiver
    {
        public enum NodeState
        {
            None,
            InActive,
            Active,
            Cleared,
            Hide
        }
        [SerializeReference, VisibleOnly] private Dungeon dungeon;
        [SerializeField, VisibleOnly] private int nodeId;
        [SerializeField, VisibleOnly] private int floor;
        [SerializeField, VisibleOnly] private DungeonNodeTypes type;
        [FormerlySerializedAs("preset")] [SerializeField, VisibleOnly] private DungeonNodeBehaviour behaviour;
        [SerializeField] private NodeState state = NodeState.None;
        [SerializeField] private bool isVisited = false;
        [SerializeField] private bool isVisible = false;
        

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
        public DungeonNodeBehaviour Behaviour 
        {
            get => behaviour;
            set => behaviour = value;
        }
        
        public NodeState State 
        {
            get => state;
            set => state = value;
        }
        
        public bool IsVisited 
        {
            get => isVisited;
            set => isVisited = value;
        }
        
        public bool IsVisible 
        {
            get => isVisible;
            set => isVisible = value;
        }
        
        [field:SerializeField] public bool IsBranchStart { get; set; } = false;
        [field:SerializeField] public bool IsInBranch { get; set; } = false;
        [field:SerializeField] public bool IsBranchEnd { get; set; } = false;
        [field:SerializeReference] public List<DungeonNode> PreviousNodes { get; private set; } = new List<DungeonNode>();
        [field:SerializeReference] public List<DungeonNode> NextNodes { get; private set; } = new List<DungeonNode>();

        private DungeonNode()
        {
            
        }

        public static DungeonNode CreateVoid()
        {
            return new DungeonNode(-1, -1, DungeonNodeTypes.None, null);
        }
        public DungeonNode(int nodeId, int floor, DungeonNodeTypes type, DungeonNodeBehaviour behaviour)
        {
            NodeId = nodeId;
            Floor = floor;
            Type = type;
            Behaviour = behaviour;
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
            if (Behaviour != null)
            {
                return $"NodeId: {NodeId}, Floor: {Floor}, Type: {Type}, Preset: {Behaviour.GetType().Name}";
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