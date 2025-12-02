using Cardevil.Attributes;
using Cardevil.DebugConsole;
using Cardevil.Dungeon.Build;
using Cardevil.Dungeon.DungeonFactories;
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
        [SerializeField] private DungeonUI dungeonUI = null;
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
                        Debug.LogError("No DungeonUI found in the scene.");
                    }
                }
                return dungeonUI;
            }
        }
        public int CurrentDungeonIndex
        {
            get => currentDungeonIndex;
            // set => currentDungeonIndex = value;
        }

        public DungeonConfigurationSO CurrentDungeonConfiguration => dungeonConfigurations[currentDungeonIndex];
        public Dungeon CurrentDungeon => GetDungeon(currentDungeonIndex);
        public DungeonNode CurrentNode => currentNode;
        public DungeonNode PreviousNode => previousNode;

        
        public void Init()
        {
            specialNodeManager = new SpecialNodeManager();

            // TODO : DungeonConfig 로드
            
            // UI 기반으로 던전 생성
            CreateDungeons();
            
            // 첫 번째 던전으로 초기화
            if (dungeons.Count > 0)
            {
                SetCurrentDungeon(0);
            }
            else
            {
                Debug.LogWarning("[DungeonManager] No dungeons created during initialization.");
                currentDungeonIndex = -1;
            }
        }

        private void CreateDungeons()
        {
            dungeons.Clear();
            Debug.Log("[DungeonManager] Creating dungeons from UI...");
            var buildHelpers = UI.GetComponentsInChildren<DungeonBuildHelperUI>();
            foreach (DungeonBuildHelperUI buildHelper in buildHelpers)
            {
                Dungeon dungeon = buildHelper.BuildDungeon();
                dungeons.Add(dungeon);
                Debug.Log($"[DungeonManager] Dungeon {dungeon.DungeonId} created with {dungeon.Nodes.Count} nodes.");
                Debug.Log(dungeon.GetDebugString());
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
            Debug.LogWarning($"[DungeonManager] Dungeon with ID {id} not found");
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
        /// 현재 던전을 인덱스로 설정
        /// </summary>
        public void SetCurrentDungeon(int index)
        {
            if (index < 0 || index >= dungeons.Count)
            {
                Debug.LogError($"[DungeonManager] Invalid dungeon index: {index}");
                return;
            }
            currentDungeonIndex = index;
            Debug.Log($"[DungeonManager] Set current dungeon to index {index}, ID: {CurrentDungeon?.DungeonId}");
            UI?.UpdateShowingDungeon(CurrentDungeon.DungeonId);
        }
        
        public void EnterNode(DungeonNode node)
        {
            if (node == null)
            {
                Debug.LogWarning("[DungeonManager] No dungeon node provided.");
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

                currentNode.Behaviour.OnExit(new NodeExitInfo() { IsCleared = true });
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
            
            LogEx.Log($"Entering node: {node.NodeId}");
            
            // 프리셋 실행
            if (node.Behaviour != null)
            {
                node.Behaviour.OnEnter();
            }
            else
            {
                LogEx.LogWarning($"No preset assigned to node {node.NodeId}.");
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
                LogEx.LogWarning("No current dungeon node to exit.");
                return;
            }
            ExitNode(currentNode, exitInfo);
        }
        
        /// <summary>
        /// 특정 노드에서 나가기
        /// </summary>
        public void ExitNode(DungeonNode node, NodeExitInfo exitInfo)
        {
            if (node == null)
            {
                LogEx.LogWarning("No dungeon node provided.");
                return;
            }
            if (node.Behaviour == null)
            {
                LogEx.LogWarning($"No behaviour assigned to node {node.NodeId}.");
                return;
            }
            
            LogEx.Log($"Exiting node: {node.NodeId}");
            
            node.Behaviour.OnExit(exitInfo);
            
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
                LogEx.LogWarning("No current dungeon node to enable blackmarket.");
                return;
            }
            LogEx.Log($"Enabling blackmarket on node: {currentNode.NodeId}");
            
            // 암시장 생성 시도
            if (specialNodeManager.TryCreateSpecialNode(currentNode, out var specialNode))
            {
                CurrentDungeon.Nodes.Add(specialNode);
                specialNode.Dungeon = CurrentDungeon;
                LogEx.Log($"Blackmarket node created after node {currentNode.NodeId}");
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
                LogEx.LogError($"[DungeonManager] Cannot start progress for dungeon {dungeonId} - not found");
                return;
            }
            
            currentProgress = new DungeonProgress(dungeonId, dungeon.RootNode.NodeId);
            
            // 루트 노드를 사용 가능 상태로 설정
            if (dungeon.RootNode != null)
            {
                dungeon.RootNode.State = NodeState.Available;
            }
            
            LogEx.Log($"[DungeonManager] Started new progress for dungeon {dungeonId}");
        }
        
        /// <summary>
        /// 진행 상태 로드
        /// </summary>
        public void LoadProgress(DungeonProgress progress)
        {
            if (progress == null)
            {
                LogEx.LogWarning("[DungeonManager] Cannot load null progress");
                return;
            }
            
            currentProgress = progress;
            var dungeon = GetDungeonById(progress.dungeonId);
            if (dungeon == null)
            {
                LogEx.LogError($"[DungeonManager] Cannot load progress - dungeon {progress.dungeonId} not found");
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
            
            LogEx.Log($"Loaded progress for dungeon {progress.dungeonId}");
        }
        
        // ============================================
        // Console Commands (디버그용)
        // ============================================
        
        /// <summary>
        /// 콘솔 명령어: 현재 던전을 인덱스로 설정
        /// </summary>
        [ConsoleCommand("dungeonSetCurrent", "Sets the current dungeon by index.", "dungeonSetCurrent <index>", new string[]{"0","1","2"})]
        public static void SetCurrentDungeonCommand(int idx)
        {
            DungeonManager dm = Managers.Dungeon;
            if (dm == null)
            {
                Console.MessageError("DungeonManager not found in Managers.");
                return;
            }
            
            if (idx < 0 || idx >= dm.dungeons.Count)
            {
                Console.MessageError($"Invalid dungeon index {idx}. Valid range: 0-{dm.dungeons.Count - 1}");
                return;
            }
            
            dm.SetCurrentDungeon(idx);
            Console.Message($"Current dungeon set to index {idx} (ID: {dm.CurrentDungeon?.DungeonId}).");
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
    }
}