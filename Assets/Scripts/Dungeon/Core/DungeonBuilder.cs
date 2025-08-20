using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace Cardevil.Dungeon
{
    public class DungeonBuilder
    {
        private int dungeonId;
        private DungeonConfigurationSO dungeonConfiguration;
        private DungeonNode rootNode;
        private List<DungeonNode> nodes;
        private DungeonNode currentNode;
        
        public DungeonNode RootNode
        {
            get => rootNode;
            set => rootNode = value;
        }
        public DungeonNode CurrentNode
        {
            get => currentNode;
        }
        
        public DungeonBuilder(int dungeonId, DungeonConfigurationSO dungeonConfiguration)
        {
            rootNode = new DungeonNode(0, 0, DungeonNodeTypes.None, null);
            nodes = new List<DungeonNode>();
            currentNode = rootNode;
            this.dungeonId = dungeonId;
            this.dungeonConfiguration = dungeonConfiguration;
            nodes.Add(rootNode); // 루트 노드를 초기 노드로 추가
        }
        
        public DungeonBuilder AddNode(DungeonNodeTypes type, DungeonNodePreset preset = null, bool moveNode = true)
        {
            int newNodeId = nodes.Count + 1;
            var newNode = new DungeonNode(newNodeId, currentNode.Floor + 1, type, preset);
            nodes.Add(newNode);
            currentNode.NextNodes.Add(newNode);
            newNode.PreviousNodes.Add(currentNode);
            newNode.IsInBranch = _isBranching; // 현재 분기 여부 설정
            if (moveNode)
            {
                currentNode = newNode;
            }
            return this;
        }
        
        /// <summary>
        /// other를 기존 빌더에 잇는다.
        /// other의 루트는 제거된다.
        /// 해당 빌더는 재사용하면 안된다.
        /// </summary>
        /// <param name="otherBuilder"></param>
        /// <returns></returns>
        [Obsolete("브랜치 지원안함. 테스트 안됨.")]
        public DungeonBuilder Append(DungeonBuilder otherBuilder)
        {
            int deltaFloor = currentNode.Floor;
            
            // 루트 노드를 제외한 다른 노드들을 추가
            for(int i = 1; i < otherBuilder.nodes.Count; i++)
            {
                var otherNode = otherBuilder.nodes[i];
                otherNode.Floor = otherNode.Floor + deltaFloor; 
                otherNode.NodeId = nodes.Count + 1; 
                nodes.Add(otherNode);
            }
            
            // 루트 노드의 다음 노드들을 현재 노드에 연결
            foreach (var node in RootNode.NextNodes)
            {
                if (node == otherBuilder.rootNode) continue; 
                node.PreviousNodes.Clear(); 
                node.Floor += deltaFloor;
                nodes.Add(node);
                currentNode.NextNodes.Add(node);
                node.PreviousNodes.Add(currentNode);
            }
            
            return this;
        }
        
        private DungeonNode branchStartNode = null;
        private bool _isBranching = false;
        private List<DungeonNode> branchEndNodes = new List<DungeonNode>();

        /// <summary>
        /// 분기 시작. 이후 AddNode를 통해 브랜치 작성
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public DungeonBuilder BranchStart()
        {
            if (_isBranching)
            {
                throw new System.InvalidOperationException("A branch is already started. Call BranchEnd to finish the current branch.");
            }
            branchStartNode = currentNode;
            branchEndNodes.Clear();
            _isBranching = true;
            return this;
        }
        
        /// <summary>
        /// 분기 끝. CurrentNode는 브랜치 시작 노드로 돌아감.
        /// </summary>
        /// <returns></returns>
        public DungeonBuilder BranchEnd()
        {
            if (_isBranching == false)
            {
                throw new System.InvalidOperationException("No branch has been started. Call BranchStart to begin a branch.");
            }
            branchEndNodes.Add(currentNode);
            currentNode = branchStartNode; // 현재 노드를 브랜치 시작 노드로 되돌림
            _isBranching = false;
            return this;
        }

        /// <summary>
        /// 브랜치의 끝에 새 노드를 추가하고, 모든 브랜치를 병합
        /// </summary>
        /// <returns></returns>
        public DungeonBuilder AddNodeAndMergeBranch(DungeonNodeTypes type, DungeonNodePreset preset = null, bool moveNode = true)
        {
            DungeonNode highestNode = null;
            foreach (var endNode in branchEndNodes)
            {
                if (highestNode == null || endNode.Floor > highestNode.Floor)
                {
                    highestNode = endNode;
                }
            }
            currentNode = highestNode ?? currentNode;
            AddNode(type, preset, false);
            var newNode = nodes[nodes.Count - 1]; // 새로 추가된 노드
            
            branchStartNode.IsBranchStart = true; // 브랜치 시작 노드 설정
            // 브랜치 끝 노드에 새 노드 연결
            foreach (var endNode in branchEndNodes)
            {
                endNode.IsBranchEnd = true; // 브랜치 끝 노드 설정
                endNode.NextNodes.Add(newNode);
                newNode.PreviousNodes.Add(endNode);
            }
            
            if (moveNode)
            {
                currentNode = newNode;
            }
            
            
            
            // 브랜치 초기화
            branchStartNode = null;
            _isBranching = false;
            branchEndNodes.Clear();
            
            return this;
        }


        
        public DungeonBuilder MovePrevious(int index = 0)
        {
            if (currentNode.PreviousNodes.Count == 0)
            {
                throw new System.InvalidOperationException("No previous nodes available.");
            }
            if (index < 0 || index >= currentNode.PreviousNodes.Count)
            {
                throw new System.ArgumentOutOfRangeException(nameof(index), "Index is out of range for previous nodes.");
            }
            currentNode = currentNode.PreviousNodes[index];
            return this;
        }
        
        public DungeonBuilder MoveNext(int index = 0)
        {
            if (currentNode.NextNodes.Count == 0)
            {
                throw new System.InvalidOperationException("No next nodes available.");
            }
            if (index < 0 || index >= currentNode.NextNodes.Count)
            {
                throw new System.ArgumentOutOfRangeException(nameof(index), "Index is out of range for next nodes.");
            }
            currentNode = currentNode.NextNodes[index];
            return this;
        }
        
        public DungeonBuilder MoveToFloor(int floor, int index = 0)
        {
            List<DungeonNode> floorNodes = nodes.FindAll(node => node.Floor == floor);
            if (index < 0 || index >= floorNodes.Count)
            {
                throw new System.ArgumentOutOfRangeException(nameof(index), "Index is out of range for the specified floor.");
            }
            currentNode = floorNodes[index];
            return this;
        }
        
        public Dungeon Build()
        {
            if (nodes.Count == 0)
            {
                throw new System.InvalidOperationException("No nodes have been added to the dungeon.");
            }
            Dungeon result = new Dungeon
            {
                dungeonId = dungeonId,
                dungeonConfiguration = dungeonConfiguration,
                RootNode = rootNode,
                Nodes = nodes
            };
            foreach (var node in nodes)
            {
                node.Initialize();
            }
            return result;
        }
        
        
    }
}