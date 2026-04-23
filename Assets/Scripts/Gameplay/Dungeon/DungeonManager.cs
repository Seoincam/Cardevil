using Cardevil.Core.Attributes;
using Cardevil.Core.Bootstrap;
using Cardevil.Core.Events.ExecEvent;
using Cardevil.Core.Utils;
using Cardevil.Gameplay.Dungeon.Build;
using Cardevil.Gameplay.Dungeon.Core;
using Cardevil.Gameplay.Dungeon.Node;
using Cardevil.Gameplay.Dungeon.NodePresets;
using Cardevil.Gameplay.Dungeon.UI;
using Cardevil.Gameplay.Root;
using Cardevil.Test.DebugConsole;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Console = Cardevil.Test.DebugConsole.Console;
using Object = UnityEngine.Object;

namespace Cardevil.Gameplay.Dungeon
{
    [Serializable]
    public class DungeonManager
    {
        [SerializeField] private List<DungeonConfigurationSO> dungeonConfigurations = new List<DungeonConfigurationSO>();
        
        [SerializeReference] private List<Core.Dungeon> dungeons = new List<Core.Dungeon>();
        [SerializeField] private DungeonUI dungeonUI;
        [FormerlySerializedAs("dungeonTransitionUI")] [SerializeField] private DungeonTransition dungeonTransition;
        [NonSerialized] private DungeonNode currentNode;
        [NonSerialized] private DungeonNode previousNode;
        [NonSerialized] private DungeonProgress currentProgress;
        [NonSerialized] private List<DungeonProgress> savedProgresses = new();
        [NonSerialized] private int currentDungeonIndex = -1;
        
        [Header("Debug")]
        [SerializeField] bool doInstantClear = false;
        
        private bool _canGoNext = true;


        public bool DoInstantClear => doInstantClear;
        public bool CanGoNext
        {
            get => _canGoNext;
            set => _canGoNext = value;
        }
        
        // ExecEventBus를 사용하므로 Events 인스턴스는 제거됨
        // 이벤트 구독: ExecEventBus<NodeEnteredEventArgs>.RegisterDynamic/RegisterStatic
        // 이벤트 발생: ExecEventBus<NodeEnteredEventArgs>.InvokeMerged(args).Forget()

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

        public DungeonTransition Transition
        {
            get
            {
                if (dungeonTransition == null)
                {
                    dungeonTransition = Object.FindAnyObjectByType<DungeonTransition>(FindObjectsInactive.Include);
                    if (dungeonTransition == null)
                    {
                        LogEx.LogError("No DungeonTransitionUI found in the scene");
                    }
                }
                return dungeonTransition;
            }
        }
        
        public int CurrentDungeonId
        {
            get => currentDungeonIndex;
        }

        public Core.Dungeon CurrentDungeon => GetDungeonById(currentDungeonIndex);
        public DungeonNode CurrentNode => currentNode;
        public DungeonNode PreviousNode => previousNode;

        
        public void Init()
        {
            ResetRuntimeState();

            // TODO : DungeonConfig 로드
            
            // 1단계: UI 참조 초기화 (InitRef 호출을 위해)
            UI?.Initialize();
            
            // 2단계: UI 기반으로 던전 생성
            CreateDungeons();
            
            // 3단계: 던전이 생성된 후 UI 초기화 완료 (라인 연결 등)
            UI?.InitializeAfterDungeonCreated();
            
            // 4단계: 던전 ID 1로 초기화
            savedProgresses.Clear();
            if (dungeons.Count > 0)
            {
                StartDungeonById(1);             
            }
            else
            {
                LogEx.LogWarning("No dungeons created during initialization");
                currentDungeonIndex = -1;
            }
        }

        private void ResetRuntimeState()
        {
            currentNode = null;
            previousNode = null;
            currentProgress = null;
            currentDungeonIndex = -1;
            _canGoNext = true;
        }

        private void CreateDungeons()
        {
            dungeons.Clear();
            //TODO : 귀찮니즘의 잔재. DungeonBuildHelperUI를 최적화해서 찾는 방법으로 바꾸는게 좋긴함
            var buildHelpers = UI.GetComponentsInChildren<DungeonBuildHelperUI>(true);
            foreach (DungeonBuildHelperUI buildHelper in buildHelpers)
            {
                Core.Dungeon dungeon = buildHelper.BuildDungeon();
                dungeons.Add(dungeon);
            }
        }

        public Core.Dungeon GetDungeon(int id)
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
        public Core.Dungeon GetDungeonById(int dungeonId)
        {
            return GetDungeon(dungeonId);
        }
        
        public int GetNextDungeonId(int dungeonDungeonId)
        {
            return dungeonDungeonId + 1;
        }
        
        public void StartDungeonById(int dungeonId)
        {
            currentNode = null;
            previousNode = null;
            SetCurrentDungeonById(dungeonId);
            var dungeon = GetDungeonById(dungeonId);
            if (dungeon?.RootNode != null)
            {
                dungeon.RootNode.State = NodeState.Available;
            }
            else
            {
                LogEx.LogError($"Root 노드가 null");
            }
            StartNewProgress(dungeonId);
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
        public bool TryEnterNode(DungeonNode node)
        {
            if (node == null)
            {
                LogEx.LogWarning("Node is null");
                return false;
            }

            if (!CanGoNext)
            {
                LogEx.Log("Cannot enter next node right now.");
                return false;
            }

            if (node.State != NodeState.Available)
            {
                LogEx.LogWarning($"Cannot enter node {node.NodeId} - state is {node.State}");
                return false;
            }

            if (currentNode != null)
            {
                if (currentNode.State == NodeState.Completed || currentNode.State == NodeState.Passed || currentNode.State == NodeState.Hidden)
                {
                    previousNode = currentNode;
                    currentNode = null;
                }
                else
                {
                    LogEx.LogWarning($"[DungeonManager] 현재 노드 {currentNode.NodeId}가 아직 종료되지 않았습니다.");
                    return false;
                }
            }
            
            // 새 노드 Enter 처리
            currentNode = node;
            currentNode.State = NodeState.Current;
            
            // 이전 노드의 다른 선택지들을 Passed 상태로 변경
            if (previousNode != null)
            {
                MarkUnselectedNodesAsPassed(previousNode, node);
            }
            
            // 진행 상태 업데이트
            if (currentProgress != null)
            {
                currentProgress.VisitNode(node.NodeId);
            }
            
            // 프리셋 실행
            if (node.Preset != null)
            {
                node.Preset.OnEnter(node);
            }
            
            // 이벤트 발생
            var enterArgs = NodeEnteredEventArgs.Get();
            enterArgs.Init(node);
            ExecEventBus<NodeEnteredEventArgs>.InvokeMergedAndDispose(enterArgs).Forget();

            if (DoInstantClear)
            {
                CompleteCurrentNode(new NodeExitInfo() { IsCleared = true });
            }

            return true;
        }

        public void EnterNode(DungeonNode node)
        {
            TryEnterNode(node);
        }

        /// <summary>
        /// 현재 노드를 완료 처리합니다.
        /// </summary>
        public void CompleteCurrentNode(NodeExitInfo exitInfo)
        {
            if (currentNode == null)
            {
                LogEx.LogWarning("No current dungeon node to exit");
                return;
            }
            
            CompleteNode(currentNode, exitInfo);
        }

        public void ExitCurrentNode(NodeExitInfo exitInfo)
        {
            CompleteCurrentNode(exitInfo);
        }

        private void CompleteNode(DungeonNode node, NodeExitInfo exitInfo)
        {
            if (node == null)
            {
                LogEx.LogWarning("Node is null");
                return;
            }
            if (node.Preset == null)
            {
                LogEx.LogWarning($"No preset assigned to node {node.NodeId}");
                return;
            }
            
            node.Preset.OnExit(node, exitInfo);
            
            if (exitInfo.IsCleared)
            {
                node.State = NodeState.Completed;
                
                ActivateNextNodes(node);
            }

            HandlePostClearFlow(node, exitInfo);
            previousNode = node;
            
            // RequiresClearToProgress가 false인 노드는 currentNode를 유지
            if (!node.RequiresClearToProgress || exitInfo.IsCleared)
            {
                currentNode = null;
            }
            
            // 이벤트 발생
            var args = NodeExitedEventArgs.Get();
            args.Init(node, exitInfo);
            ExecEventBus<NodeExitedEventArgs>.InvokeMergedAndDispose(args).Forget();
        }

        private void HandlePostClearFlow(DungeonNode node, NodeExitInfo exitInfo)
        {
            if (!exitInfo.IsCleared)
            {
                return;
            }

            if (node.Type != DungeonNodeTypes.FinalBoss)
            {
                return;
            }

            int nextDungeonId = GetNextDungeonId(node.Dungeon.DungeonId);
            if (GetDungeonById(nextDungeonId) != null)
            {
                StartDungeonById(nextDungeonId);
            }
        }
        
        /// <summary>
        /// 클리어한 노드의 다음 노드들을 활성화합니다
        /// </summary>
        private void ActivateNextNodes(DungeonNode clearedNode)
        {
            foreach (var nextNode in clearedNode.NextNodes)
            {
                if (nextNode.State == NodeState.Locked)
                {
                    // 블랙마켓 노드 처리
                    if (nextNode.Type == DungeonNodeTypes.BlackMarket && nextNode.Preset is BlackMarketNodePreset blackMarketPreset)
                    {
                        bool appeared = blackMarketPreset.ShouldAppear();
                        
                        void SetAvailableNexts()
                        {
                            foreach (var afterBlackMarketNode in nextNode.NextNodes)
                            {
                                if (afterBlackMarketNode.State == NodeState.Locked)
                                {
                                    afterBlackMarketNode.State = NodeState.Available;
                                }
                            } 
                        }
                        if (blackMarketPreset.AutoConnectNextNodes)
                        {
                            SetAvailableNexts();
                        }
                        
                        if (appeared)
                        {
                            nextNode.State = NodeState.Available;
                            LogEx.Log($"[DungeonManager] 블랙마켓 노드 {nextNode.NodeId} 활성화");
                        }
                        else
                        {
                            nextNode.State = NodeState.Hidden;
                            SetAvailableNexts();
                            LogEx.Log($"[DungeonManager] 블랙마켓 노드 {nextNode.NodeId} 숨김 처리");
                        }
                    }
                    else
                    {
                        nextNode.State = NodeState.Available;
                    }
                }
            }
        }
        
        /// <summary>
        /// 선택하지 않은 노드들을 Passed 상태로 변경합니다
        /// </summary>
        /// <param name="fromNode">출발한 노드 (이전 노드)</param>
        /// <param name="selectedNode">선택한 노드 (현재 노드)</param>
        private void MarkUnselectedNodesAsPassed(DungeonNode fromNode, DungeonNode selectedNode)
        {
            if (fromNode == null || selectedNode == null) return;
            
            // 이전 노드의 다음 노드들 중 선택하지 않은 노드들을 Passed로 변경
            foreach (var nextNode in fromNode.NextNodes)
            {
                if (nextNode.NodeId == selectedNode.NodeId) continue;
                
                // 암시장이 나타나지 않은 경우는 무시 
                if (nextNode.Type == DungeonNodeTypes.BlackMarket && nextNode.State == NodeState.Locked)
                {
                    LogEx.Log($"[DungeonManager] 암시장 노드 {nextNode.NodeId}는 나타나지 않아 무시");
                    continue;
                }
                
                // Available 상태인 노드만 Passed로 변경
                if (nextNode.State == NodeState.Available)
                {
                    nextNode.State = NodeState.Passed;
                    
                    // Progress에 기록
                    if (currentProgress != null)
                    {
                        currentProgress.PassNode(nextNode.NodeId);
                    }
                    
                    LogEx.Log($"[DungeonManager] 노드 {nextNode.NodeId}를 Passed 상태로 변경");
                }
            }
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
            savedProgresses.Add(currentProgress);
            
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
            
            var dungeon = GetDungeonById(progress.dungeonId);
            if (dungeon == null)
            {
                LogEx.LogError($"Cannot load progress - dungeon {progress.dungeonId} not found");
                return;
            }

            currentProgress = progress;
            previousNode = null;
            currentNode = null;
            currentDungeonIndex = progress.dungeonId;
            UI?.UpdateShowingDungeon(progress.dungeonId);

            foreach (var node in dungeon.Nodes)
            {
                if (node == null)
                {
                    continue;
                }

                node.State = NodeState.Locked;
            }

            if (dungeon.RootNode != null)
            {
                dungeon.RootNode.State = NodeState.Available;
            }
            
            // 방문한 노드들을 완료 상태로 설정
            foreach (int nodeId in progress.visitedNodeIds)
            {
                var node = dungeon.GetNode(nodeId);
                if (node != null)
                {
                    node.State = NodeState.Completed;
                    ActivateNextNodes(node);
                }
            }
            
            // 지나친 노드들을 Passed 상태로 설정
            foreach (int nodeId in progress.passedNodeIds)
            {
                var node = dungeon.GetNode(nodeId);
                if (node != null)
                {
                    node.State = NodeState.Passed;
                }
            }
            
            // 현재 노드 설정
            var progressNode = dungeon.GetNode(progress.currentNodeId);
            if (progressNode != null)
            {
                progressNode.State = NodeState.Current;
                currentNode = progressNode;
            }
        }

        #region Console Commands

        [ConsoleCommand("UnlockAllCurrentDungeonNodes", "Unlocks all nodes in the current dungeon.", "UnlockAllCurrentDungeonNodes")]
        public static void Console_UnlockAllCurrentDungeonNodes()
        {
            DungeonManager dm = CardevilCore.GameFlow.World?.Dungeon;
            if (dm == null)
            {
                Console.MessageError("DungeonManager not found in Managers.");
                return;
            }
            
            var currentDungeon = dm.CurrentDungeon;
            if (currentDungeon == null)
            {
                Console.MessageError("No current dungeon to unlock nodes in.");
                return;
            }

            foreach (var node in currentDungeon.Nodes) 
            {
                node.State = NodeState.Available;
            }
            
            Console.Message("All nodes in the current dungeon have been unlocked.");
        }

        [ConsoleCommand("UnlockAllDungeons", "Unlocks all nodes in all dungeons.", "UnlockAllDungeons")]
        public static void Console_UnlockAllDungeons()
        {
            DungeonManager dm = CardevilCore.GameFlow.World?.Dungeon;
            if (dm == null)
            {
                Console.MessageError("DungeonManager not found in Managers.");
                return;
            }
            
            foreach (var dungeon in dm.dungeons) 
            {
                foreach (var node in dungeon.Nodes) 
                {
                    node.State = NodeState.Available;
                }
            }
            
            Console.Message("All nodes in all dungeons have been unlocked.");
        }

        [ConsoleCommand("SetCurrentDungeon", "Sets the current dungeon by ID.", "SetCurrentDungeon <dungeonId>",
            new[] { "1", "2", "3" })]
        public static void Console_SetCurrentDungeon(int dungeonId)
        {
            DungeonManager dm = CardevilCore.GameFlow.World?.Dungeon;
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
        
         /*
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
         */
        #endregion

        
    }
}

