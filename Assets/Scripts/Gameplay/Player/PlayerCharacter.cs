using Cardevil.Core.Bootstrap;
using Cardevil.Core.Turn;
using Cardevil.Core.Utils;
using Cardevil.DebugConsole;
using Cardevil.Gameplay.Core;
using Cardevil.Gameplay.Entities;
using Cardevil.Gameplay.Field;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Scripting;
using Console = Cardevil.DebugConsole.Console;

namespace Cardevil.Gameplay.Player
{
    /// <summary>
    /// 플레이어 캐릭터 클래스
    /// </summary>
    public class PlayerCharacter : MonoBehaviour, IPlayerControl, ITurnPlayer
    {
        [Header("Debug")]
        [SerializeField] protected bool _isDebugMode = false;
        [SerializeField] protected bool _initTileManually = false;
        [SerializeField] protected TileVector _initialTile;
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
        public PlayerStatus PlayerStatus => CardevilCore.Game.PlayerStatus;
        public PlayerVisual PlayerVisual => _playerVisual;
        public Transform VisualTransform => _playerVisual.transform;

        private void Awake()
        {
            if(_entity == null)
            {
                _entity = GetComponent<Entity>();
            }
        }
        
        // private void Start()
        // {
        //     if (_initTileManually)
        //     {
        //         // _entity.Init(Field.GetTile(_initialTile));
        //         // Bootstrapper.Instance.Game.Player = this; // 게임 매니저에 플레이어 설정
        //     }
        // }

        public void Init(Field.Field field)
        {
            Field = field;
            
            if(_entity == null)
            {
                _entity = GetComponent<Entity>();
            }
            
            if (_initTileManually)
            {
                _entity.Init(Field.GetTile(_initialTile));
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
                        MoveWithAnim(direction).Forget();
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

        public bool debugReflect = false;
        
        
        /// <summary>
        /// 실제로 이동하지 않고, MoveWithAnim과 동일한 로직으로 플레이어가 최종적으로 도착할 타일을 계산합니다.
        /// </summary>
        /// <param name="field">현재 필드</param>
        /// <param name="startTile">출발 타일</param>
        /// <param name="direction">이동 방향</param>
        /// <param name="distance">이동 거리</param>
        /// <returns>
        /// (resultTile: 최종 도착 타일, wrapped: wrap 발생 여부, reflected: 반사 발생 여부)
        /// </returns>
        public static (Tile resultTile, bool wrapped, bool reflected) GetTrueTileInDirection(
            Field.Field field, Tile startTile, Direction direction, int distance = 1)
        {
            // wrap 여부와 함께 이동하려는 타일을 가져옴
            Tile desiredTile = field.GetTileByDirectionWrap(startTile, direction, out bool wrapped, distance);

            // 해당 타일에 반사 엔티티가 있는지 확인
            bool reflected = desiredTile.HasEntity(e =>
                e.TryGetComponent<IReflectorEntity>(out var reflector) && reflector.DoReflect);

            // 반사가 없으면 desiredTile이 최종 목적지
            if (!reflected)
            {
                return (desiredTile, wrapped, false);
            }

            // 반사가 있으면 반대 방향으로 distance만큼 이동한 타일이 최종 목적지
            //   -            반사: 제자리(startTile)로 돌아옴
            //   - Wrap + 반사: wrap된 desiredTile에서 반대방향으로 이동
            Tile reflectedTile = field.GetTileByDirectionWrap(desiredTile, direction.Opposite(), out _, distance);
            return (reflectedTile, wrapped, true);
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
            // 원래 타일
            Tile originalTile = _entity.CurrentTile;
            // 이동하고 싶은 타일 (반사 판정 전)
            Tile desiredTile = Field.GetTileByDirectionWrap(_entity.CurrentTile, direction, out bool wrapped, distance);

            // GetTrueTileInDirection과 동일한 로직으로 최종 목적지 계산
            var (movedTile, _, reflected) = GetTrueTileInDirection(Field, originalTile, direction, distance);

            // debugReflect 플래그 처리 (디버그 전용)
            if (debugReflect && !reflected)
            {
                reflected = true;
                movedTile = originalTile;
            }

            using var listHandle = ListPool<IReflectorEntity>.Get(out var entityList);
            desiredTile.GetEntitiesWithComponent<IReflectorEntity>((e) => e.DoReflect, ref entityList);

            void NotifyReflect()
            {
                foreach (var reflecter in entityList)
                {
                    reflecter.OnReflect();
                }
            }

            _entity.MoveTo(movedTile, false);
            
            // 단순한 이동 애니메이션
            async UniTask MoveTask(Vector3 endPosition, Action<float> Update = null)
            {
                PlayerVisual.MoveDirection = direction.ToTileVector().ToVector2IntDirect();
                PlayerVisual.IsRunning = true;
                var tween = transform.DOMove(endPosition, 1f / MoveSpeed).SetEase(Ease.Linear);
                if (Update != null)
                {
                    tween.OnUpdate(() =>
                    {
                        float progress = tween.ElapsedPercentage();
                        Update(progress);
                    });
                }
                await tween.ToUniTask();
                PlayerVisual.IsRunning = false;
            }
            
            // 이동 후 떨어지는 애니메이션
            async UniTask MoveFallTask()
            {
                Vector3 startPosition = transform.position;
                // 여러칸 이동했더라도 한칸만 이동한 것으로 취급 TODO : 다수 이동시 문제 있음
                Vector3 outPosition = Field.GetTileCenterWorld(originalTile.Coordinate + direction.ToTileVector());
                Vector3 targetPosition = desiredTile.transform.position;
                outPosition.y = targetPosition.y = startPosition.y; // Y 축은 유지
                await MoveTask(outPosition, (progress) =>
                {
                    PlayerVisual.ShadowFadeAlpha = 1f - Mathf.Clamp01(progress * 2f);
                });
                PlayerVisual.IsFalling = true;
                PlayerVisual.ShadowFadeAlpha = 0f;
                
                await VisualTransform.DOShakePosition(playerCharacterSetting.coyoteTime,
                    strength:playerCharacterSetting.coyoteShakeStrength * new Vector3(1,0,1),
                    vibrato:playerCharacterSetting.coyoteShakeCount,
                    randomness:90,
                    snapping:false,
                    fadeOut:true).ToUniTask();
                await UniTask.Delay(playerCharacterSetting.fallDelayAfterCoyoteTimeMs);
                
                var sequence = DOTween.Sequence();
                // 떨어지는 시퀀스
                sequence.Append(VisualTransform.DOLocalMoveY(- playerCharacterSetting.fallHeight,
                    playerCharacterSetting.fallDuration).SetEase(playerCharacterSetting.fallEase));
                // 떨어지는 페이드아웃
                float fadeoutStartTime = Mathf.Lerp(0, playerCharacterSetting.fallDuration,
                    playerCharacterSetting.fallFadeStartRatio);
                sequence.Insert(fadeoutStartTime,
                    DOTween.To(() => PlayerVisual.FadeAlpha, value => PlayerVisual.FadeAlpha = value,0, playerCharacterSetting.fallDuration-fadeoutStartTime));
                
                // 다시 떨어지는 시퀀스
                sequence.AppendCallback(() =>
                {
                    transform.position = targetPosition;
                    VisualTransform.localPosition = Vector3.up * playerCharacterSetting.fallHeight;
                });
                sequence.Append(VisualTransform.DOLocalMoveY(0,
                    playerCharacterSetting.fallDuration).SetEase(playerCharacterSetting.fallEase));
                float fadeinEndTime = Mathf.Lerp(0, playerCharacterSetting.fallDuration,
                    playerCharacterSetting.fallFadeEndRatio);
                sequence.Join(DOTween.To(() => PlayerVisual.FadeAlpha, value => PlayerVisual.FadeAlpha = value,1,fadeinEndTime));
                sequence.Join(DOTween.To(() => PlayerVisual.ShadowFadeAlpha, value => PlayerVisual.ShadowFadeAlpha = value,1,fadeinEndTime));
                await sequence.ToUniTask();
                PlayerVisual.IsFalling = false;
            }
            
            // 이동하다 반사되는 애니메이션
            async UniTask MoveReflectTask()
            {
                Vector3 startPosition = transform.position;
                Vector3 desiredTilePosition = desiredTile.transform.position;
                desiredTilePosition.y = startPosition.y;
                PlayerVisual.MoveDirection = direction.ToTileVector().ToVector2IntDirect();
                PlayerVisual.IsRunning = true;
                Sequence sequence = DOTween.Sequence();
                sequence.Append(transform.DOMove(desiredTilePosition, 1f / MoveSpeed).SetEase(Ease.Linear));
                sequence.AppendCallback(NotifyReflect);
                sequence.Append(transform.DOMove(startPosition, 0.5f / MoveSpeed).SetEase(Ease.Linear));
                sequence.Join(VisualTransform.DOLocalJump(
                    Vector3.zero,
                    0.25f,
                 1,
                    1f / MoveSpeed).SetEase(Ease.Linear));
                await sequence.ToUniTask();
                PlayerVisual.IsRunning = false;
            }

            async UniTask MoveReflectFallTask()
            {
                // 시작 위치
                Vector3 startPosition = transform.position;
                // 첫번째 삐져나간 위치
                // 여러칸 이동했더라도 한칸만 이동한 것으로 취급 TODO : 다수 이동시 문제 있음
                Vector3 outPosition = Field.GetTileCenterWorld(originalTile.Coordinate + direction.ToTileVector());
                // 가고자 했던 위치
                Vector3 desiredTilePosition = desiredTile.transform.position;
                // 두번째 삐져나간 위치
                Vector3 out2Position = Field.GetTileCenterWorld(desiredTile.Coordinate + direction.Opposite().ToTileVector());
                outPosition.y = desiredTilePosition.y = startPosition.y; // Y 축은 유지
                await MoveTask(outPosition, (progress) =>
                {
                    PlayerVisual.ShadowFadeAlpha = 1f - Mathf.Clamp01(progress * 2f);
                });
                PlayerVisual.IsFalling = true;
                PlayerVisual.ShadowFadeAlpha = 0f;
                
                await VisualTransform.DOShakePosition(playerCharacterSetting.coyoteTime,
                    strength:playerCharacterSetting.coyoteShakeStrength * new Vector3(1,0,1),
                    vibrato:playerCharacterSetting.coyoteShakeCount,
                    randomness:90,
                    snapping:false,
                    fadeOut:true).ToUniTask();
                await UniTask.Delay(playerCharacterSetting.fallDelayAfterCoyoteTimeMs);
                
                // 1차 추락
                var sequence = DOTween.Sequence();
                // 떨어지기
                sequence.Append(VisualTransform.DOLocalMoveY(- playerCharacterSetting.fallHeight,
                    playerCharacterSetting.fallDuration).SetEase(playerCharacterSetting.fallEase));
                // 떨어지는 페이드아웃
                float fadeoutStartTime = Mathf.Lerp(0, playerCharacterSetting.fallDuration,
                    playerCharacterSetting.fallFadeStartRatio);
                sequence.Insert(fadeoutStartTime,
                    DOTween.To(() => PlayerVisual.FadeAlpha, value => PlayerVisual.FadeAlpha = value,0, playerCharacterSetting.fallDuration-fadeoutStartTime));
                
                // 다시 떨어지는 시퀀스
                sequence.AppendCallback(() =>
                {
                    transform.position = desiredTilePosition;
                    VisualTransform.localPosition = Vector3.up * playerCharacterSetting.fallHeight;
                });
                sequence.Append(VisualTransform.DOLocalMoveY(0,
                    playerCharacterSetting.fallDuration).SetEase(playerCharacterSetting.fallEase));
                float fadeinEndTime = Mathf.Lerp(0, playerCharacterSetting.fallDuration,
                    playerCharacterSetting.fallFadeEndRatio);
                sequence.Join(DOTween.To(() => PlayerVisual.FadeAlpha, value => PlayerVisual.FadeAlpha = value,1,fadeinEndTime));
                sequence.Join(DOTween.To(() => PlayerVisual.ShadowFadeAlpha, value => PlayerVisual.ShadowFadeAlpha = value,1,fadeinEndTime));
                await sequence.ToUniTask();
                
                // 충돌함. 반사
                NotifyReflect();
                var reflectSequence = DOTween.Sequence();
                // 튕기는중
                reflectSequence.Append(VisualTransform.DOLocalJump(
                    Vector3.zero,
                    0.25f,
                 1,
                    1f / MoveSpeed).SetEase(Ease.Linear));
                reflectSequence.Join(transform.DOMove(out2Position, 0.5f / MoveSpeed).SetEase(Ease.Linear));
                // 이후 추락
                reflectSequence.Append(
                    VisualTransform.DOLocalMoveY(- playerCharacterSetting.fallHeight,
                        playerCharacterSetting.fallDuration).SetEase(playerCharacterSetting.fallEase));
                // 떨어지는 페이드아웃
                float reflectFadeoutStartTime = Mathf.Lerp(0, playerCharacterSetting.fallDuration,
                    playerCharacterSetting.fallFadeStartRatio);
                reflectSequence.Insert(reflectFadeoutStartTime,
                    DOTween.To(() => PlayerVisual.FadeAlpha, value => PlayerVisual.FadeAlpha = value,0, playerCharacterSetting.fallDuration-reflectFadeoutStartTime));
                // 다시 떨어지는 시퀀스
                reflectSequence.AppendCallback(() =>
                {
                    transform.position = movedTile.transform.position;
                    VisualTransform.localPosition = Vector3.up * playerCharacterSetting.fallHeight;
                });
                reflectSequence.Append(VisualTransform.DOLocalMoveY(0,
                    playerCharacterSetting.fallDuration).SetEase(playerCharacterSetting.fallEase));
                float reflectFadeinEndTime = Mathf.Lerp(0, playerCharacterSetting.fallDuration,
                    playerCharacterSetting.fallFadeEndRatio);
                reflectSequence.Join(DOTween.To(() => PlayerVisual.FadeAlpha, value => PlayerVisual.FadeAlpha = value,1,reflectFadeinEndTime));
                reflectSequence.Join(DOTween.To(() => PlayerVisual.ShadowFadeAlpha, value => PlayerVisual.ShadowFadeAlpha = value,1,reflectFadeinEndTime));
                await reflectSequence.ToUniTask();
                
                
                PlayerVisual.IsFalling = false;
            }

            if (!wrapped)
            {
                if (reflected)
                {
                    await MoveReflectTask();
                }
                else
                {
                    Vector3 startPosition = transform.position;
                    Vector3 targetPosition = desiredTile.transform.position;
                    targetPosition.y = startPosition.y; // Y 축은 유지
                    await MoveTask(targetPosition);
                }
            }
            else
            {
                if (reflected)
                {
                    await MoveReflectFallTask();
                }
                else
                {
                    await MoveFallTask();
                }
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

        #region ITurnPlayer
        
        public bool IsDead { get; }
        public TileVector Position { get; }
        
        public async UniTask OnMoveAsync(Card.InStage.PlayerMoveArgs args, CancellationToken cancellationToken)
        {
            await MoveWithAnim(args.Direction, 1);
            await UniTask.Delay(300, cancellationToken: cancellationToken);
        }

        public async UniTask AttackAsync(float damage)
        {
            LogEx.Log("Player Attacks!");

            await UniTask.Delay(100);

            LogEx.Log($"플레이어의 공격 : {damage} 피해. 구현 아직");
            PlayerVisual.PlayAttackAnimation();
        }

        public async UniTask TakeDamageAsync(float damage)
        {
            LogEx.Log($"Player takes {damage} damage!");
            PlayerStatus.TakeDamage((int)damage);
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