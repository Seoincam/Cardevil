﻿using Cardevil.Attributes;
 using Cardevil.Utils;
 using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Cardevil.Dungeon
{
    /// <summary>
    /// 던전 내 개별 노드를 나타내는 클래스
    /// 각 노드는 전투, 회복, 보스 등 다양한 타입을 가질 수 있습니다.
    /// </summary>
    [Serializable]
    public class DungeonNode : ISerializationCallbackReceiver
    {
        [SerializeReference, VisibleOnly] private Dungeon dungeon;
        [FormerlySerializedAs("nodeId")] [SerializeField, VisibleOnly] private int nodeInternalId;
        [SerializeField, VisibleOnly] private int floor;
        [SerializeField, VisibleOnly] private DungeonNodeTypes type;
        [FormerlySerializedAs("preset")] [SerializeField, VisibleOnly] private DungeonNodeBehaviour behaviour;
        [SerializeField, VisibleOnly] private NodeState state = NodeState.Locked;

        public event Action<NodeState> OnStateChanged;
        
        
        public Dungeon Dungeon
        {
            get => dungeon;
            set => dungeon = value;
        }
        public int NodeId
        {
            get => nodeInternalId;
            set => nodeInternalId = value;
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
            set
            {
                state = value;
                OnStateChanged?.Invoke(state);
            }
        }
        
        [field:SerializeField] public bool IsBranchStart { get; set; } = false;
        [field:SerializeField] public bool IsInBranch { get; set; } = false;
        [field:SerializeField] public bool IsBranchEnd { get; set; } = false;
        [field:NonSerialized] public List<DungeonNode> PreviousNodes { get; private set; } = new List<DungeonNode>();
        [field:NonSerialized] public List<DungeonNode> NextNodes { get; private set; } = new List<DungeonNode>();
        [NonSerialized] public List<DungeonNode> OriginNextNodes; // 암시장 같은 특별 노드에서 원래의 다음 노드들을 저장하기 위함

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
            PreviousNodes = new List<DungeonNode>();
            NextNodes = new List<DungeonNode>();
            state = NodeState.Locked;
        }

        /// <summary>
        /// 던전 빌드 후에 실행 - 필요한 초기화 로직 수행
        /// </summary>
        public void Initialize()
        {
            // 노드 상태 초기화
            state = NodeState.Locked;
            
            
            Debug.Log($"[DungeonNode] Node {NodeId} initialized. Type: {Type}, NextNodes: {NextNodes.Count}, PrevNodes: {PreviousNodes.Count}");
        }
        
        /// <summary>
        /// 다음 노드로 연결 (양방향)
        /// </summary>
        public void LinkTo(DungeonNode nextNode)
        {
            if (nextNode == null) 
            {
                Debug.LogWarning($"[DungeonNode] Cannot link to null node from {NodeId}");
                return;
            }
            if (!NextNodes.Contains(nextNode))
            {
                NextNodes.Add(nextNode);
            }
            if (!nextNode.PreviousNodes.Contains(this))
            {
                nextNode.PreviousNodes.Add(this);
            }
        }
        
        /// <summary>
        /// 다음 노드를 리스트 맨 앞에 연결 (우선순위가 높은 경로)
        /// </summary>
        public void LinkToFirst(DungeonNode firstNode)
        {
            if (firstNode == null) 
            {
                LogEx.LogWarning($"Cannot link to null node from {NodeId}");
                return;
            }
            if (!NextNodes.Contains(firstNode))
            {
                NextNodes.Insert(0, firstNode);
            }
            if (!firstNode.PreviousNodes.Contains(this))
            {
                firstNode.PreviousNodes.Add(this);
            }
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
            // Serialization 전에 필요한 작업이 있으면 여기에 추가
        }

        public void OnAfterDeserialize()
        {
            // Deserialization 후 NonSerialized 필드 복원
            // NextNodes와 PreviousNodes는 NonSerialized이므로 재초기화 필요
            if (PreviousNodes == null) PreviousNodes = new List<DungeonNode>();
            if (NextNodes == null) NextNodes = new List<DungeonNode>();
        }
    }
}