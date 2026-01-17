using Cardevil.Attributes;
using Cardevil.Cards.Core;
using Cardevil.Core.Bootstrap;
using Cardevil.Core.Turn;
using Cardevil.Dungeon;
using Cardevil.Enemy;
using Cardevil.Utils;
using Cysharp.Threading.Tasks;
using Cardevil.Ingame.Field;
using Cardevil.Ingame.Player;
using Cardevil.SceneManagement;
using Unity.VisualScripting;
using UnityEngine;

namespace Cardevil.Core.Root
{
    /// <summary>
    /// 전투 스테이지 루트 컨트롤러.
    /// 카드, 턴, 적, 필드, 플레이어 초기화 및 전투 흐름 제어.
    /// </summary>
    public class StageRoot : MonoBehaviour
    {
        [SerializeField] private TurnManager turn;
        [SerializeField] private EnemySpawner _enemySpawner;

        [Header("References")]
        [field: SerializeField, VisibleOnly(EditableIn.EditMode)] public Field Field { get; private set; }
        [field: SerializeField, VisibleOnly(EditableIn.EditMode)] public PlayerCharacter Player { get; set; } // 임시 플레이어

        // public int TurnOrder => _turn.TurnOrder;
        
        private GameFlowManager.StageEnterContext _context;

        
        private async void Awake()
        {
            CardevilCore.Instance.GameFlow.Stage = this;
            
            // TODO: 로딩을 bootstrapper or stage에서 관리할지 고민하기
            
            _enemySpawner = new EnemySpawner();
            turn = new TurnManager();
            // turn.TurnLoopEnded += OnTurnLoopEnded;
            
            Player.Init(Field);
            await InitAsync();
            await EnterStageAsync();
        }

        private async UniTask InitAsync()
        {
            _context = CardevilCore.Instance.GameFlow.Context;
        }
        
        /// <summary>
        /// 전투 스테이지 진입 비동기 처리.
        /// 스테이지 ID 기반 적 스폰, 필드 및 턴 매니저 구성 후 턴 루프 시작.
        /// </summary>
        private async UniTask EnterStageAsync()
        {
            // TODO: 얘를 외부에서 호출되도록 해야겠음.
            
            // TODO : 필드 초기화 - @machamy
            Field.InitField(3,3, Random.Range(0,4));
            
            _enemySpawner.ConfigStageMobData(_context.stageId);
            if (!_enemySpawner.TrySpawn(out var enemy))
            {
                LogEx.LogError($"Failed to spawn Enemy. stage Id: {_context.stageId}");
                return;
            }
            
            enemy.Init(Field);
            turn.Initialize(_enemySpawner, CardFlowController.Build(), Player, enemy);
            turn.EnterLoopAsync().Forget();
        }

        /// <summary>
        /// 턴 루프 종료 처리.
        /// 던전 노드 퇴장 처리 및 스테이지 씬 언로드.
        /// </summary>
        private void OnTurnLoopEnded()
        {
            // TODO: 보상창 등 나와야함.
            // TODO: 변경사항 세이브 해야함.
            
            LogEx.Log("스테이지 종료");
            
            var exitInfo = new NodeExitInfo() { IsCleared = true };
            CardevilCore.Instance.GameFlow.World.Dungeon.ExitCurrentNode(exitInfo);
            SceneLoader.UnloadSceneAsync(Scenes.Stage).Forget();
        }
    }
}