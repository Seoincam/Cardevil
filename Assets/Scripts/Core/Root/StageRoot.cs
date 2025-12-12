using Cardevil.Attributes;
using Cardevil.Cards.System;
using Cardevil.Core.Bootstrap;
using Cardevil.Enemy;
using Cardevil.Systems;
using Cardevil.Utils;
using Cysharp.Threading.Tasks;
using Cardevil.Ingame.Field;
using Cardevil.Ingame.Player;
using UnityEngine;

namespace Cardevil.Core.Root
{
    public class StageRoot : MonoBehaviour
    {
        [field: SerializeField] public CardManager Card { get; private set; }
        [SerializeField] private TurnManager _turn;
        [SerializeField] private EnemySpawner _enemySpawner;

        [Header("References")]
        [field: SerializeField, VisibleOnly] public Field Field { get; private set; }
        [field: SerializeField, VisibleOnly] public PlayerCharacter Player { get; set; } // 임시 플레이어

        public ITurnEnemy Enemy => _turn.Enemy;
        public int TurnOrder => _turn.TurnOrder;
        
        private GameFlowManager.StageEnterContext _context;

        
        private async void Awake()
        {
            // TODO: 로딩을 bootstrapper or stage에서 관리할지 고민하기
            
            // Reference 
            Field = Object.FindAnyObjectByType<Field>();
            if (!Field)
            {
                LogEx.LogError("Field not found in the scene.");
                return;
            }
            
            await InitAsync();
            
        }

        private async UniTask InitAsync()
        {
            _context = Bootstrapper.Instance.GameFlow.Context;
            
            var game = Bootstrapper.Instance.Game;
            var cardLibrary = game.CardLibrary;

            
            _enemySpawner = new EnemySpawner();
            _turn = new TurnManager();
            _turn.Init(_enemySpawner); // TODO: turn에 spawner를 줘야할지 고민하기
            
            await Card.InitAsync(cardLibrary);
            
        }

        /// <summary>
        /// 전투 스테이지에 진입합니다.
        /// </summary>
        private async UniTask EnterStageAsync()
        {
            _enemySpawner.ConfigStageMobData(_context.stageId);
            if (!_enemySpawner.TrySpawn(out var enemy))
            {
                LogEx.LogError($"Failed to spawn Enemy. stage Id: {_context.stageId}");
                return;
            }
            
            // _turn.Register(Card.BuildFlow(), );
            _turn.StartLoop();
        }
    }
}