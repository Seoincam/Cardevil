using Cardevil.Core.Bootstrap;
using Cardevil.Core.Events.ExecEvent;
using Cardevil.Core.Utils;
using Cardevil.Gameplay.Dungeon;
using Cardevil.Gameplay.Dungeon.Core;
using Cardevil.Gameplay.Dungeon.Node;
using Cardevil.Gameplay.Root;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Cardevil.Core
{
    public class StageLoopEndEventArgs : ExecEventArgs<StageLoopEndEventArgs>
    {
    }

    public class SpecialSceneLoopEndEventArgs : ExecEventArgs<SpecialSceneLoopEndEventArgs>
    {
        public SceneManagement.Scenes Scene { get; private set; }

        public void Init(SceneManagement.Scenes scene)
        {
            Scene = scene;
        }

        public override void Clear()
        {
            base.Clear();
            Scene = default;
        }
    }

    public class SpecialSceneExitStartedEventArgs : ExecEventArgs<SpecialSceneExitStartedEventArgs>
    {
        public SceneManagement.Scenes Scene { get; private set; }

        public void Init(SceneManagement.Scenes scene)
        {
            Scene = scene;
        }

        public override void Clear()
        {
            base.Clear();
            Scene = default;
        }
    }

    [Serializable]
    public class GameFlowManager
    {
        [Serializable]
        public struct StageEnterContext
        {
            public string stageId;
            public List<string> mobIds;
        }

        [Serializable]
        public struct SpecialSceneEnterContext
        {
            public SceneManagement.Scenes scene;
            public string roomId;
            public int nodeId;
            public int floor;
            public DungeonNodeTypes sourceNodeType;
        }

        [field: SerializeField] public StageEnterContext Context { get; private set; }
        [field: SerializeField] public SpecialSceneEnterContext SpecialContext { get; private set; }

        public WorldRoot World { get; set; }
        public StageRoot Stage { get; set; }
        public Gameplay.SpecialScenes.SpecialSceneRoot SpecialScene { get; set; }

        public void Init()
        {
            ExecEventBus<NodeEnteredEventArgs>.RegisterStatic(10, HandleNodeEnteredTask);
            ExecEventBus<StageLoopEndEventArgs>.RegisterStatic(10, HandleStageLoopEndTask);
            ExecEventBus<SpecialSceneExitStartedEventArgs>.RegisterStatic(10, HandleSpecialSceneExitStartedTask);
            ExecEventBus<SpecialSceneLoopEndEventArgs>.RegisterStatic(10, HandleSpecialSceneLoopEndTask);
        }

        public async UniTask HandleNodeEnteredTask(NodeEnteredEventArgs args, CancellationToken ct)
        {
            LogEx.Log("Dungeon node selected");

            switch (args.Node.Type)
            {
                case DungeonNodeTypes.Mob:
                case DungeonNodeTypes.MiniBoss:
                case DungeonNodeTypes.FinalBoss:
                    Context = new StageEnterContext
                    {
                        stageId = args.Node.RoomData.RoomID,
                        mobIds = args.Node.RoomData.MobList
                    };

                    await World.TransitionToStageAsync(Context, ct);

                    Stage.Initialize(
                        Context,
                        CardevilCore.PlayerStatus,
                        CardevilCore.Game.CardRepository,
                        CardevilCore.Game.ScoreProviderRegistry);
                    await Stage.StartStageAsync();
                    break;
                case DungeonNodeTypes.Heal:
                    await TransitionToSpecialSceneAsync(args.Node, SceneManagement.Scenes.Heal, ct);
                    break;
                case DungeonNodeTypes.Shop:
                    await TransitionToSpecialSceneAsync(args.Node, SceneManagement.Scenes.Shop, ct);
                    break;
                case DungeonNodeTypes.BlackMarket:
                    await TransitionToSpecialSceneAsync(args.Node, SceneManagement.Scenes.BlackMarket, ct);
                    break;
                case DungeonNodeTypes.Random:
                    await TransitionToSpecialSceneAsync(args.Node, SceneManagement.Scenes.GoodPlace, ct);
                    break;
                default:
                    if (!args.Node.RequiresClearToProgress)
                    {
                        World.Dungeon?.CompleteCurrentNode(new NodeExitInfo { IsCleared = true });
                    }
                    break;
            }
        }

        /// <summary>
        /// 특별 씬으로 전환하는 공통 로직. 씬별로 필요한 정보를 컨텍스트에 담아서 전달한다.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="scene"></param>
        /// <param name="ct"></param>
        private async UniTask TransitionToSpecialSceneAsync(DungeonNode node, SceneManagement.Scenes scene, CancellationToken ct)
        {
            SpecialContext = new SpecialSceneEnterContext
            {
                scene = scene,
                roomId = node.RoomId,
                nodeId = node.NodeId,
                floor = node.Floor,
                sourceNodeType = node.Type,
            };

            await World.TransitionToSpecialSceneAsync(SpecialContext, ct);
        }

        /// <summary>
        /// 특별 씬에서 나올 때 공통으로 처리해야 하는 로직. 특별 씬이 끝났다는 것을 로그로 남기고, 던전 노드를 클리어 처리한 뒤 월드로 돌아간다.
        /// </summary>
        /// <param name="scene"></param>
        /// <param name="ct"></param>
        public async UniTask HandleSpecialSceneLoopEndTask(SpecialSceneLoopEndEventArgs args, CancellationToken ct)
        {
            LogEx.Log($"Special scene completed: {args.Scene}");

            var world = World;
            if (world == null)
            {
                LogEx.LogError("GameFlowManager: World is null while handling special scene loop end.");
                return;
            }

            world.Dungeon?.CompleteCurrentNode(new NodeExitInfo { IsCleared = true });
            await world.ReturnToWorldFromSpecialSceneAsync(args.Scene, ct);
        }

        public async UniTask HandleSpecialSceneExitStartedTask(SpecialSceneExitStartedEventArgs args, CancellationToken ct)
        {
            var world = World;
            if (world == null)
            {
                LogEx.LogError("GameFlowManager: World is null while preparing special scene return.");
                return;
            }

            await world.PrepareWorldForSpecialSceneReturnAsync(args.Scene, ct);
        }

        public async UniTask HandleStageLoopEndTask(StageLoopEndEventArgs args, CancellationToken ct)
        {
            LogEx.Log("Stage loop ended");

            var stage = Stage;
            var world = World;
            if (stage == null)
            {
                LogEx.LogError("GameFlowManager: Stage is null while handling stage loop end.");
                return;
            }

            if (world == null)
            {
                LogEx.LogError("GameFlowManager: World is null while handling stage loop end.");
                return;
            }
            
            DungeonManager dm = world.Dungeon;
            if (dm == null)
            {
                LogEx.LogError("StageRoot: Dungeon manager is null during stage shutdown.");
                return;
            }

            dm.CompleteCurrentNode(new NodeExitInfo() { IsCleared = true });

            try
            {
                await world.ReturnToWorldFromStageAsync(ct);
            }
            finally
            {
                if (ReferenceEquals(Stage, stage))
                {
                    Stage = null;
                }
            }
        }
    }
}
