using Cardevil.Attributes;
using Cardevil.DebugConsole;
using Cardevil.Dungeon.Build;
using Cardevil.Dungeon.UI;
using Cardevil.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;
using Console = Cardevil.DebugConsole.Console;
using Object = UnityEngine.Object;

namespace Cardevil.Dungeon
{
    [Serializable]
    public class DungeonManager
    {
        [SerializeField] private List<DungeonConfigurationSO> dungeonConfigurations = new List<DungeonConfigurationSO>();
        
        [SerializeReference] private List<Dungeon> dungeons = new List<Dungeon>();
        [SerializeField] private DungeonUI dungeonUI;
        [SerializeField, VisibleOnly] private DungeonNode currentNode;
        [SerializeField, VisibleOnly] private DungeonNode previousNode;
        [SerializeField, VisibleOnly] private DungeonProgress currentProgress;
        private int currentDungeonIndex = -1;
        private SpecialNodeManager specialNodeManager;
        
        // 이벤트 시스템
        [NonSerialized] private DungeonEvents events;
        public DungeonEvents Events
        {
            get
            {
                if (events == null)
                {
                    events = new DungeonEvents();
                }
                return events;
            }
        }

        public DungeonUI UI
        {
            get
            {
                if (dungeonUI == null)
                {
                    dungeonUI = Object.FindAnyObjectByType<DungeonUI>(FindObjectsInactive.Include);
                    if (dungeonUI == null)
                    {
                        LogEx.LogError("No DungeonUI found in the scene");
                    }
                }
                return dungeonUI;
            }
        }
        public int CurrentDungeonId
        {
            get => currentDungeonIndex;
        }

        public Dungeon CurrentDungeon => GetDungeonById(currentDungeonIndex);
        public DungeonNode CurrentNode => currentNode;
        public DungeonNode PreviousNode => previousNode;

        
        public void Init()
        {
            specialNodeManager = new SpecialNodeManager();

            // TODO : DungeonConfig 로드
            
            // 1단계: UI 참조 초기화 (InitRef 호출을 위해)
            UI?.Initialize();
            
            // 2단계: UI 기반으로 던전 생성
            CreateDungeons();
            
            // 3단계: 던전이 생성된 후 UI 초기화 완료 (라인 연결 등)
            UI?.InitializeAfterDungeonCreated();
            
            // 4단계: 던전 ID 1로 초기화
            if (dungeons.Count > 0)
            {
                // 먼저 Root 노드를 Available로 변경
                var dungeon = GetDungeonById(1);
                if (dungeon?.RootNode != null)
                {
                    dungeon.RootNode.State = NodeState.Available;
                }
                else
                {
                    LogEx.LogError($"Root 노드가 null");
                }
                
                SetCurrentDungeonById(1);
            }
            else
            {
                LogEx.LogWarning("No dungeons created during initialization");
                currentDungeonIndex = -1;
            }
        }

        private void CreateDungeons()
        {
            dungeons.Clear();
            var buildHelpers = UI.GetComponentsInChildren<DungeonBuildHelperUI>();
            foreach (DungeonBuildHelperUI buildHelper in buildHelpers)
            {
                Dungeon dungeon = buildHelper.BuildDungeon();
                dungeons.Add(dungeon);
            }
        }

        public Dungeon GetDungeon(int id)
        {
            foreach (var dungeon in dungeons)
            {
                if (dungeon != null && dungeon.DungeonId == id)
                {
                    return dungeon;
                }
            }
            LogEx.LogWarning($"Dungeon with ID {id} not found");
            return null;
        }

        /// <summary>
        /// 던전 ID로 던전 찾기 (GetDungeon과 동일하지만 명시적인 이름)
        /// </summary>
        public Dungeon GetDungeonById(int dungeonId)
        {
            return GetDungeon(dungeonId);
        }

        /// <summary>
        /// 현재 던전을 ID로 설정
        /// </summary>
        public void SetCurrentDungeonById(int dungeonId)
        {
            var dungeon = GetDungeonById(dungeonId);
            if (dungeon == null)
            {
                LogEx.LogError($"Invalid dungeon ID: {dungeonId}");
                return;
            }
            currentDungeonIndex = dungeonId;
            UI?.UpdateShowingDungeon(dungeonId);
        }
        
        /// <summary>
        /// 노드에 진입합니다
        /// </summary>
        public void EnterNode(DungeonNode node)
        {
            if (node == null)
            {
                LogEx.LogWarning("Node is null");
                return;
            }
            
            // 이전 노드 Exit 처리
            if (currentNode != null && currentNode.Behaviour != null)
            {
                // 이전 노드가 특별 노드였다면, 원래 경로로 복원
                if (currentNode.OriginNextNodes != null)
                {
                    specialNodeManager.ClearSpecialNode(currentNode);
                }

                currentNode.Behaviour.OnExit(currentNode, new NodeExitInfo() { IsCleared = true });
                currentNode.State = NodeState.Completed;
                Events.RaiseNodeExited(currentNode);
                
                // 이전 노드로 저장
                previousNode = currentNode;

                // 암시장 생성 시도
                if (specialNodeManager.TryCreateSpecialNode(currentNode, out var specialNode))
                {
                    // 특별 노드가 생성되면, 던전의 노드 리스트에 임시로 추가
                    CurrentDungeon.Nodes.Add(specialNode);
                    specialNode.Dungeon = CurrentDungeon;
                }
            }
            
            // 새 노드 Enter 처리
            currentNode = node;
            currentNode.State = NodeState.Current;
            
            // 진행 상태 업데이트
            if (currentProgress != null)
            {
                currentProgress.VisitNode(node.NodeId);
            }
            
            // 다음 노드들을 사용 가능 상태로 변경
            foreach (var nextNode in currentNode.NextNodes)
            {
                if (nextNode.State == NodeState.Locked)
                {
                    nextNode.State = NodeState.Available;
                }
            }
            
            // 프리셋 실행
            if (node.Behaviour != null)
            {
                node.Behaviour.OnEnter(node);
            }
            
            // 이벤트 발생
            Events.RaiseNodeEntered(node);
        }
        
        /// <summary>
        /// 현재 노드에서 나가기
        /// </summary>
        public void ExitCurrentNode(NodeExitInfo exitInfo)
        {
            if (currentNode == null)
            {
                LogEx.LogWarning("No current dungeon node to exit");
                return;
            }
            
            ExitNode(currentNode, exitInfo);
        }
        
        /// <summary>
        /// 노드에서 나가기
        /// </summary>
        public void ExitNode(DungeonNode node, NodeExitInfo exitInfo)
        {
            if (node == null)
            {
                LogEx.LogWarning("Node is null");
                return;
            }
            if (node.Behaviour == null)
            {
                LogEx.LogWarning($"No behaviour assigned to node {node.NodeId}");
                return;
            }
            
            node.Behaviour.OnExit(node, exitInfo);
            
            if (exitInfo.IsCleared)
            {
                node.State = NodeState.Completed;
            }
            
            previousNode = node;
            currentNode = null;
            
            Events.RaiseNodeExited(node);
        }
        
        /// <summary>
        /// 현재 노드에 암시장 활성화
        /// </summary>
        public void EnableBlackmarketOnCurrentNode()
        {
            if (currentNode == null)
            {
                LogEx.LogWarning("No current dungeon node to enable blackmarket");
                return;
            }
            
            // 암시장 생성 시도
            if (specialNodeManager.TryCreateSpecialNode(currentNode, out var specialNode))
            {
                CurrentDungeon.Nodes.Add(specialNode);
                specialNode.Dungeon = CurrentDungeon;
            }
        }
        
        /// <summary>
        /// 새로운 던전 진행 상태 시작
        /// </summary>
        public void StartNewProgress(int dungeonId)
        {
            var dungeon = GetDungeonById(dungeonId);
            if (dungeon == null)
            {
                LogEx.LogError($"Cannot start progress for dungeon {dungeonId} - not found");
                return;
            }
            
            currentProgress = new DungeonProgress(dungeonId, dungeon.RootNode.NodeId);
            
            // 루트 노드를 사용 가능 상태로 설정
            if (dungeon.RootNode != null)
            {
                dungeon.RootNode.State = NodeState.Available;
            }
        }
        
        /// <summary>
        /// 진행 상태 로드
        /// </summary>
        public void LoadProgress(DungeonProgress progress)
        {
            if (progress == null)
            {
                LogEx.LogWarning("Cannot load null progress");
                return;
            }
            
            currentProgress = progress;
            var dungeon = GetDungeonById(progress.dungeonId);
            if (dungeon == null)
            {
                LogEx.LogError($"Cannot load progress - dungeon {progress.dungeonId} not found");
                return;
            }
            
            // 방문한 노드들을 완료 상태로 설정
            foreach (int nodeId in progress.visitedNodeIds)
            {
                var node = dungeon.GetNode(nodeId);
                if (node != null)
                {
                    node.State = NodeState.Completed;
                }
            }
            
            // 현재 노드 설정
            var progressNode = dungeon.GetNode(progress.currentNodeId);
            if (progressNode != null)
            {
                progressNode.State = NodeState.Current;
            }
        }

        #region Console Commands
        
        /// <summary>
        /// 콘솔 명령어: 현재 던전을 ID로 설정
        /// </summary>
        [ConsoleCommand("dungeonSetCurrent", "Sets the current dungeon by ID.", "dungeonSetCurrent <dungeonId>", new []{"1","2","3"})]
        public static void SetCurrentDungeonCommand(int dungeonId)
        {
            DungeonManager dm = Managers.Dungeon;
            if (dm == null)
            {
                Console.MessageError("DungeonManager not found in Managers.");
                return;
            }
            
            var dungeon = dm.GetDungeonById(dungeonId);
            if (dungeon == null)
            {
                Console.MessageError($"Invalid dungeon ID {dungeonId}.");
                return;
            }
            
            dm.SetCurrentDungeonById(dungeonId);
            Console.Message($"Current dungeon set to ID {dungeonId}.");
        }

        /// <summary>
        /// 콘솔 명령어: 현재 노드 클리어
        /// </summary>
        [ConsoleCommand("dungeonClearCurrentNode", "Clears the current dungeon node.", "dungeonClearCurrentNode")]
        public static void ClearCurrentNode()
        {
            DungeonManager dm = Managers.Dungeon;
            if (dm == null)
            {
                Console.MessageError("DungeonManager not found in Managers.");
                return;
            }
            
            if (dm.CurrentNode == null)
            {
                Console.MessageError("No current dungeon node to clear.");
                return;
            }
            
            dm.ExitCurrentNode(new NodeExitInfo() { IsCleared = true });
            Console.Message($"Current dungeon node (ID: {dm.PreviousNode?.NodeId}) cleared.");
        }
        
        [ConsoleCommand("dungeonPrintDebugInfo", "Prints debug information about all dungeons.", "dungeonPrintDebugInfo")]
        public static void PrintDungeonDebugInfo()
        {
            DungeonManager dm = Managers.Dungeon;
            if (dm == null)
            {
                Console.MessageError("DungeonManager not found in Managers.");
                return;
            }

            for (int i = 0; i < dm.dungeons.Count; i++)
            {
                var dungeon = dm.dungeons[i];
                Console.Message($"Dungeon Index: {i}, ID: {dungeon.DungeonId}");
                Console.Message(dungeon.GetDebugString());
            }
        }
        #endregion
    }
}

