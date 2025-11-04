using Cardevil.Cards.Evaluations;
using Cardevil.DebugConsole;
using Cardevil.Ingame.Field;
using Cardevil.Systems;
using Cardevil.Utils;
using Cardevil.Utils.Directions;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.Scripting;
using Console = Cardevil.DebugConsole.Console;

namespace Cardevil.Ingame.Entities
{
    /// <summary>
    /// 플레이어 캐릭터 클래스
    /// </summary>
    public class PlayerCharacter : MonoBehaviour, IPlayerControl, ITurnPlayerAction, ITurnPlayerMove
    {
        [Header("Debug")]
        [SerializeField] protected bool _isDebugMode = false;
        [SerializeField] protected bool _initTileManually = false;
        [SerializeField] protected Tile _initialTile;
        [Header("Settings")]
        [SerializeField] protected float _moveSpeed = 5f; // 이동 속도
        [Header("References")]
        [SerializeField] protected Entity _entity;
        [SerializeField] protected PlayerVisual _playerVisual;

        
        public float MoveSpeed => _moveSpeed;
        private void Reset()
        {
            _entity = GetComponent<Entity>();
            _playerVisual = GetComponentInChildren<PlayerVisual>(); 
        }

        public Entity Entity => _entity;
        public PlayerStatus PlayerStatus => Managers.Game.PlayerStatus;
        public PlayerVisual PlayerVisual => _playerVisual;
        private void Awake()
        {
            if(_entity == null)
            {
                _entity = GetComponent<Entity>();
            }
        }
        
        private void Start()
        {
            if (_initTileManually)
            {
                if (_initialTile == null)
                {
                    LogEx.LogError("Initial tile is not set. Please assign a tile in the inspector.");
                    return;
                }
                _entity.Init(_initialTile);
                Managers.Game.Player = this; // 게임 매니저에 플레이어 설정
            }
        }

        public void Update()
        {
            if (_isDebugMode)
            {
                if(Input.anyKeyDown)
                {
                    int horizontal = (int)Input.GetAxisRaw("Horizontal");
                    int vertical = (int)Input.GetAxisRaw("Vertical");
                    // print($"Horizontal: {horizontal}, Vertical: {vertical}");
                    if (horizontal != 0 || vertical != 0)
                    {
                        Direction direction = Direction.None;
                        if (horizontal > 0) direction = Direction.Right;
                        else if (horizontal < 0) direction = Direction.Left;
                        else if (vertical > 0) direction = Direction.Up;
                        else if (vertical < 0) direction = Direction.Down;
                        LogEx.Log($"Moving in direction: {direction}");
                        MoveWithAnim(direction);
                    }
                }
            }
        }

        /*
         * IPlayerControl 인터페이스 구현
         * TODO : Move 메서드 개선. 현재는 즉시이동 + wrapAround 활성화
         * 
         */


        /// <summary>
        /// 플레이어를 해당 방향으로 한 칸 이동시킵니다.
        /// </summary>
        /// <remarks>
        /// transform은 바로 이동하지 않고, 애니메이션을 통해 이동합니다.
        /// </remarks>
        /// <param name="direction"></param>
        public async UniTask MoveWithAnim(Direction direction)
        {
            await MoveWithAnim(direction, 1);
        }

        /// <summary>
        /// 플레이어를 해당 방향으로 distance 칸 이동시킵니다.
        /// </summary>
        /// <remarks>
        /// transform은 바로 이동하지 않고, 애니메이션을 통해 이동합니다.
        /// </remarks>
        /// <param name="direction"></param>
        /// <param name="distance"></param>
        public async UniTask MoveWithAnim(Direction direction, int distance)
        {
            bool wrapped = false;
            Tile targetTile = _entity.CurrentTile.Field.GetTileByDirectionWrap(_entity.CurrentTile, direction, out wrapped);
            _entity.MoveTo(targetTile, false);
            async UniTask MoveTask(Vector3 endPosition)
            {
                PlayerVisual.MoveDirection = direction.ToTileVector().ToVector2IntDirect();
                PlayerVisual.IsRunning = true;
                var tween = transform.DOMove(endPosition, 1f / _moveSpeed).SetEase(Ease.Linear);
                await tween.AsyncWaitForCompletion();
                PlayerVisual.IsRunning = false;
            }
            
            Vector3 startPosition = transform.position;
            Vector3 targetPosition = _entity.CurrentTile.transform.position;
            targetPosition.y = startPosition.y; // Y 축은 유지
            await MoveTask(targetPosition);
        }

        public void MoveTo(int i, int j)
        {
            _entity.MoveTo(new TileVector(i, j), true);
        }

        public void MoveTo(TileVector tileVector)
        {
            _entity.MoveTo(tileVector, true);
        }

        public void MoveTo(Tile tile)
        {
            if (tile == null)
            {
                LogEx.LogError("Cannot move to a null tile.");
                return;
            }
            _entity.MoveTo(tile, true);
        }
        /// <summary>  플레이어의 Horinzontal Line Number </summary>
        public int GetPlayerLineNumberHorizontal() 
        {
            return Entity.Tile.i;
        }
        /// <summary>  플레이어의 Vertical Line Number </summary>
        public int GetPlayerLineNumberVertical()
        {
            return Entity.Tile.j;
        }

        #region ITurnPlayerAction, ITurnPlayerMove 구현
        public bool IsDead => Managers.Game.PlayerStatus.CurrentHp <= 0;
        public async UniTask TurnAttack()
        {
            LogEx.Log("Player Attacks!");

            await UniTask.Delay(100);
            // TODO : 적에 대한 공격 구현
            var result = Managers.Card.EvaluationResults.CurrentResult;
            LogEx.Log($"플레이어 공격 : {result.TotalDamage} 피해. 구현 아직");
            void DealDamageToEnemies()
            {
                Managers.Game.Enemy.GetDamage(result.TotalDamage);
            }
            DealDamageToEnemies();
            PlayerVisual.PlayAttackAnimation();
      
        }
        
        public void PlayerGetDamage(float amount)
        {
            LogEx.Log($"Player takes {amount} damage!");
            PlayerStatus.TakeDamage((int)amount);
            
        }

        public async UniTask TurnMove()
        {
            LogEx.Log("Player Moves!");
            var result = Managers.Card.EvaluationResults.CurrentResult;
            //TODO 이동 로직 구현
            foreach (var move in result.Moves)
            {
                if (!move.DirectionSelectState.FinalValue.HasValue)
                    continue;

                await MoveWithAnim((Direction)move.DirectionSelectState.FinalValue, move.Length);
                await UniTask.Delay(300);
            }
            LogEx.Log("Player Move Completed!");
        }
        

        #endregion



        [Preserve, ConsoleCommand("togglePlayerDebug", "Toggle player debug mode")]
        private static void TogglePlayerDebug()
        {
            var player = FindFirstObjectByType<PlayerCharacter>();
            if (player != null)
            {
                player._isDebugMode = !player._isDebugMode;
                Console.MessageInfo($"Player debug mode: {(player._isDebugMode ? "ON" : "OFF")}");
            }
            else
            {
                Console.MessageError("No PlayerCharacter found in the scene.");
            }
        }
    }
}