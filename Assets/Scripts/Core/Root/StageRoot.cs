using Cardevil.Attributes;
using Cardevil.Cards.System;
using Cardevil.Core.Bootstrap;
using Cardevil.Core.Turn;
using Cardevil.Enemy;
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
        [field: SerializeField] public Field Field { get; private set; }
        [field: SerializeField] public PlayerCharacter Player { get; set; } // 임시 플레이어

        // public int TurnOrder => _turn.TurnOrder;
        
        private GameFlowManager.StageEnterContext _context;

        
        private async void Awake()
        {
            // TODO: 로딩을 bootstrapper or stage에서 관리할지 고민하기
            
            _enemySpawner = new EnemySpawner();
            _turn = new TurnManager();
            
            Player.Init(Field);
            await InitAsync();
            await EnterStageAsync();
        }

        private async UniTask InitAsync()
        {
            _context = Bootstrapper.Instance.GameFlow.Context;
            
            var game = Bootstrapper.Instance.Game;
            var cardLibrary = game.CardLibrary;
            var enhancementData = Bootstrapper.Instance.CardEnhancementData;
            
            await Card.InitAsync(cardLibrary, enhancementData);
        }

        /// <summary>
        /// 전투 스테이지에 진입합니다.
        /// </summary>
        private async UniTask EnterStageAsync()
        {
            LogEx.Log("EnterStageAsync");
            _enemySpawner.ConfigStageMobData(_context.stageId);
            if (!_enemySpawner.TrySpawn(out var enemy))
            {
                LogEx.LogError($"Failed to spawn Enemy. stage Id: {_context.stageId}");
                return;
            }
            
            enemy.Init(Field);
            _turn.Init(_enemySpawner, Card.BuildFlow(), Player, enemy);
            _turn.StartLoop();
        }
    }
}