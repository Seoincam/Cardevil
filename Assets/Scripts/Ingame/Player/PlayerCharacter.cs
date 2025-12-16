using Cardevil.Cards.Evaluations;
using Cardevil.Core.Bootstrap;
using Cardevil.Core.Turn;
using Cardevil.Core.Turn.Interfaces;
using Cardevil.DebugConsole;
using Cardevil.Events.AsyncPriorityEvent;
using Cardevil.Events.Core;
using Cardevil.Ingame.Entities;
using Cardevil.Ingame.Field;
using Cardevil.Utils;
using Cardevil.Utils.Directions;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using JetBrains.Annotations;
using System;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.Serialization;
using Console = Cardevil.DebugConsole.Console;

namespace Cardevil.Ingame.Player
{
    /// <summary>
    /// 플레이어 캐릭터 클래스
    /// </summary>
    public class PlayerCharacter : MonoBehaviour, IPlayerControl, ITurnPlayer
    {
        [Header("Debug")]
        [SerializeField] protected bool _isDebugMode = false;
        [SerializeField] protected bool _initTileManually = false;
        [SerializeField] protected Tile _initialTile;
        [Header("Settings")] 
        [SerializeField] protected PlayerCharacterSettingSO playerCharacterSetting;
        [Header("References")]
        [SerializeField] protected Entity _entity;
        [SerializeField] protected PlayerVisual _playerVisual;


        public float MoveSpeed => playerCharacterSetting.moveSpeed;
        private void Reset()
        {
            _entity = GetComponent<Entity>();
            _playerVisual = GetComponentInChildren<PlayerVisual>(); 
        }

        public Entity Entity => _entity;
        public Field.Field Field { get; private set; }
        public PlayerStatus PlayerStatus => Bootstrapper.Instance.Game.PlayerStatus;
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
                // Bootstrapper.Instance.Game.Player = this; // 게임 매니저에 플레이어 설정
            }
        }

        public void Init(Field.Field field)
        {
            Field = field;
            
            if(_entity == null)
            {
                _entity = GetComponent<Entity>();
            }
            
            if (_initTileManually)
            {
                if (_initialTile == null)
                {
                    LogEx.LogError("Initial tile is not set. Please assign a tile in the inspector.");
                    return;
                }
                _entity.Init(_initialTile);
                // Bootstrapper.Instance.Game.Player = this; // 게임 매니저에 플레이어 설정
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
            /*
             * 1. 이동할 위치를 받아온다.
             * 2. Wrap이 아니라면, 평범하게 이동
             * 3. Wrap이라면, 특수 애니메이션
             *
             * A. 원래 가려고 했던 위치 받아오기
             * 
             */
            bool wrapped = false;
            Tile targetTile = Field.GetTileByDirectionWrap(_entity.CurrentTile, direction, out wrapped, distance);
            Tile originalTile = _entity.CurrentTile;
            _entity.MoveTo(targetTile, false);
            
            async UniTask MoveTask(Vector3 endPosition)
            {
                PlayerVisual.MoveDirection = direction.ToTileVector().ToVector2IntDirect();
                PlayerVisual.IsRunning = true;
                var tween = transform.DOMove(endPosition, 1f / MoveSpeed).SetEase(Ease.Linear);
                await tween.ToUniTask();
                PlayerVisual.IsRunning = false;
            }

            if (!wrapped)
            {
                Vector3 startPosition = transform.position;
                Vector3 targetPosition = _entity.CurrentTile.transform.position;
                targetPosition.y = startPosition.y; // Y 축은 유지
                await MoveTask(targetPosition);
            }
            else
            {
                Vector3 startPosition = transform.position;
                // 여러칸 이동했더라도 한칸만 이동한 것으로 취급
                Vector3 outPosition = Field.GetTileCenterWorld(originalTile.Coordinate + direction.ToTileVector());
                Vector3 targetPosition = _entity.CurrentTile.transform.position;
                outPosition.y = targetPosition.y = startPosition.y; // Y 축은 유지
                await MoveTask(outPosition);
                PlayerVisual.IsFalling = true;

                
                await transform.DOShakePosition(playerCharacterSetting.coyoteTime,
                    strength:playerCharacterSetting.coyoteShakeStrength * new Vector3(1,0,1),
                    vibrato:playerCharacterSetting.coyoteShakeCount,
                    randomness:90,
                    snapping:false,
                    fadeOut:true).ToUniTask();
                await UniTask.Delay(playerCharacterSetting.fallDelayAfterCoyoteTimeMs);
                
                var sequence = DOTween.Sequence();
                // 떨어지는 시퀀스
                sequence.Append(transform.DOMoveY(startPosition.y - playerCharacterSetting.fallHeight,
                    playerCharacterSetting.fallDuration).SetEase(playerCharacterSetting.fallEase));
                // 떨어지는 페이드아웃
                float fadeoutStartTime = Mathf.Lerp(0, playerCharacterSetting.fallDuration,
                    playerCharacterSetting.fallFadeStartRatio);
                sequence.Insert(fadeoutStartTime,
                    DOTween.To(() => PlayerVisual.FadeAlpha, value => PlayerVisual.FadeAlpha = value,0, playerCharacterSetting.fallDuration-fadeoutStartTime));
                
               
                // 다시 떨어지는 시퀀스
                sequence.AppendCallback(() => transform.position = targetPosition + Vector3.up * playerCharacterSetting.fallHeight);
                sequence.Append(transform.DOMoveY(targetPosition.y,
                    playerCharacterSetting.fallDuration).SetEase(playerCharacterSetting.fallEase));
                float fadeinEndTime = Mathf.Lerp(0, playerCharacterSetting.fallDuration,
                    playerCharacterSetting.fallFadeEndRatio);
                sequence.Join(DOTween.To(() => PlayerVisual.FadeAlpha, value => PlayerVisual.FadeAlpha = value,1,fadeinEndTime));
                await sequence.ToUniTask();
                PlayerVisual.IsFalling = false;
            }
            
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

        #region ITurnPlayerAction 구현
        
        public bool IsDead { get; }

        public async UniTask<AttackResult> TurnAttackAsync(IReadOnlyTurnContext ctx)
        {
            LogEx.Log("Player Attacks!");

            await UniTask.Delay(100);

            var result = ctx.CardFlow.Result;
            int damage = result.TotalDamage;
            var handRanking = result.HandRanking;

            LogEx.Log($"플레이어 공격 : {damage} 피해. 구현 아직");
            PlayerVisual.PlayAttackAnimation();
            // TODO: 공격 애니메이션도 await하기
            
            return new AttackResult(target: ctx.CurrentEnemy, handRanking, damage);
        }
        
        public void TakeDamage(int amount)
        {
            LogEx.Log($"Player takes {amount} damage!");
            PlayerStatus.TakeDamage((int)amount);
        }

        public async UniTask<Vector2Int> TurnMove(IReadOnlyTurnContext ctx)
        {
            LogEx.Log("Player Moves!");
            var result = ctx.CardFlow.Result;
            //TODO 이동 로직 구현
            foreach (var move in result.Moves)
            {
                if (!move.DirectionSelectState.FinalValue.HasValue)
                    continue;
                
                await MoveWithAnim((Direction)move.DirectionSelectState.FinalValue, move.Length);
                await UniTask.Delay(300);
            }
            LogEx.Log("Player Move Completed!");

            return new Vector2Int(Entity.Tile.i, Entity.Tile.j);
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