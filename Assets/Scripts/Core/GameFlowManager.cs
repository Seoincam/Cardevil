using Cardevil.Core.Root;
using Cardevil.Dungeon;
using Cardevil.Events.ExecEvents;
using Cardevil.SceneManagement;
using Cardevil.Utils;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Cardevil.Core
{
    [Serializable]
    public class GameFlowManager
    {
        [Serializable]
        public struct StageEnterContext
        {
            public string stageId;
            public List<string> mobIds;
            
            // TODO: 성장 계수 등 다른 요소도 넣어야할 듯.
        }

        [field: SerializeField] public StageEnterContext Context { get; private set; } = new() { stageId = "Test" };
        
        [field: SerializeField] public WorldRoot World { get; set; }
        [field: SerializeField] public StageRoot Stage { get; set; }
        

        public void Init()
        {
            // TODO: 우선 순위 정하기.
            // 가장 늦은 우선 순위로 하면 될 듯.
            ExecEventBus<NodeEnteredEventArgs>.RegisterStatic(10, RequestEnterStage);
        }

        public async UniTask RequestEnterStage(NodeEnteredEventArgs args)
        {
            LogEx.Log("던전 노드 눌림");
            // TODO: 전투, 회복, 상점 등 분기해야함.
            // TODO: Db에 접근해서 스테이지 데이터 구성.

            if (!World)
            {
                LogEx.LogError("World is null");
                return;
            }

            World.EnterStageAsync(Context).Forget();
        }
    }
}