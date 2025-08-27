using Cardevil.Cards;
using Cardevil.Ingame.Field;
using Cardevil.Systems;
using Cardevil.Utils;
using Cardevil.Utils.Directions;
using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

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
        [Header("References")]
        [SerializeField] protected Entity _entity;

        
        public Entity Entity => _entity;

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
                    Debug.LogError("Initial tile is not set. Please assign a tile in the inspector.");
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
                        Debug.Log($"Moving in direction: {direction}");
                        Move(direction);
                    }
                }
            }
        }

        /*
         * IPlayerControl 인터페이스 구현
         * TODO : Move 메서드 개선. 현재는 즉시이동 + wrapAround 활성화
         * 
         */
        public void Move(Direction[] directions)
        {
            foreach (var direction in directions)
            {
                Move(direction, 1);
            }
        }

        public void Move(Direction direction)
        {
            Move(direction, 1);
        }

        public void Move(Direction direction, int distance)
        {
            _entity.MoveDirection(direction, distance, true);
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
                Debug.LogError("Cannot move to a null tile.");
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
            Debug.Log("Player Attacks!");
            CardContext ctx = Managers.Card.handBar.Context;
            CardResult result = ctx.CurrentResult;
            await UniTask.Delay(100);
            // TODO : 적에 대한 공격 구현
            Debug.Log($"플레이어 공격 : {result.Damage} 피해. 구현 아직");
        }
        
        public void PlayerGetDamage(float amount)
        {
            Debug.Log($"Player takes {amount} damage!");
            Managers.Game.PlayerStatus.CurrentHp -= (int)amount;
        }

        public async UniTask TurnMove()
        {
            Debug.Log("Player Moves!");
            CardContext ctx = Managers.Card.handBar.Context;
            CardResult result = ctx.CurrentResult;
            //TODO 이동 로직 구현
            foreach (var move in result.Moves)
            {
                Move(move.direction, move.length);
                await UniTask.Delay(100);
            }
            Debug.Log("Player Move Completed!");
        }
        

        #endregion

    }
}