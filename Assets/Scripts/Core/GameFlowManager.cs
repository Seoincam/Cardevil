using Cardevil.Core.Bootstrap;
using Cardevil.Core.Events.ExecEvent;
using Cardevil.Core.Utils;
using Cardevil.Gameplay.Dungeon;
using Cardevil.Gameplay.Dungeon.Core;
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

    [Serializable]
    public class GameFlowManager
    {
        [Serializable]
        public struct StageEnterContext
        {
            public string stageId;
            public List<string> mobIds;
        }

        [field: SerializeField] public StageEnterContext Context { get; private set; }

        [field: SerializeField] public WorldRoot World { get; set; }
        [field: SerializeField] public StageRoot Stage { get; set; }

        public void Init()
        {
            ExecEventBus<NodeEnteredEventArgs>.RegisterStatic(10, HandleEnterStageTask);
            ExecEventBus<StageLoopEndEventArgs>.RegisterStatic(10, HandleStageLoopEndTask);
        }

        public async UniTask HandleEnterStageTask(NodeEnteredEventArgs args, CancellationToken ct)
        {
            LogEx.Log("Dungeon node selected");

            Context = new StageEnterContext
            {
                stageId = args.Node.RoomData.RoomID, 
                mobIds = args.Node.RoomData.MobList
            };
            
            await World.EnterStageAsync(Context, ct);

            Stage.Initialize(
                Context,
                CardevilCore.PlayerStatus,
                CardevilCore.Game.CardRepository,
                CardevilCore.Game.ScoreProviderRegistry);
            await Stage.EnterStageAsync();
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

            dm.ExitCurrentNode(new NodeExitInfo() { IsCleared = true });

            try
            {
                await world.ReturnFromStageAsync(ct);
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
