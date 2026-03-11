using Cardevil.Ingame.Field;
using Cardevil.Utils;
using Cardevil.Utils.Directions;
using UnityEngine;

namespace Cardevil.Ingame.Entities
{
    /// <summary>
    /// 엔티티의 서브 컴포넌트 인터페이스
    /// </summary>
    public interface IEntityComponent
    {
        public Entity Entity { get; }
    }
    
    public class Entity : MonoBehaviour
    {
        private Tile _currentTile;

        public Tile CurrentTile => _currentTile;
        public TileVector Tile => _currentTile.Coordinate;
        
        public void Init(Tile initialTile)
        {
            if (initialTile == null)
            {
                Debug.LogError("Initial tile cannot be null.");
                return;
            }

            _currentTile = initialTile;
            _currentTile.AddEntity(this);
            transform.position = new Vector3(initialTile.transform.position.x, transform.position.y, initialTile.transform.position.z);
        }


        /// <summary>
        /// 해당 타일로 엔티티를 이동
        /// </summary>
        /// <param name="tile"></param>
        /// <param name="moveTransform">transform까지 이동 여부</param>
        public void MoveTo(Tile tile, bool moveTransform = false)
        {
            if (tile == null)
            {
                Debug.LogError("Cannot move to a null tile.");
                return;
            }

            if (_currentTile != null)
            {
                _currentTile.RemoveEntity(this);
            }

            _currentTile = tile;
            _currentTile.AddEntity(this);

            if (moveTransform)
            {
                transform.position = new Vector3(tile.transform.position.x, transform.position.y, tile.transform.position.z);
            }
        }
        
        public void MoveDirection(Direction direction, int distance = 1, bool moveTransform = false)
        {
            Tile targetTile = _currentTile.Field.GetTileByDirection(CurrentTile, direction, true);
            MoveTo(targetTile, moveTransform);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tileVector"></param>
        /// <param name="moveTransform"></param>
        /// <param name="wrapAround"></param>
        public void MoveTo(TileVector tileVector, bool moveTransform = false, bool wrapAround = false)
        {
            Tile targetTile = _currentTile.Field.GetTile(tileVector);
            if (targetTile == null)
            {
                Debug.LogError($"Cannot move to tile at {tileVector} - tile does not exist.");
                return;
            }
            MoveTo(targetTile, moveTransform);
        }
        
        public void MoveTo(int i, int j, bool moveTransform = false)
        {
            Tile targetTile = _currentTile.Field.GetTile(i, j);
            if (targetTile == null)
            {
                Debug.LogError($"Cannot move to tile at ({i}, {j}) - tile does not exist.");
                return;
            }
            MoveTo(targetTile, moveTransform);
        }


        public void Kill(bool destroyGameObject = true)
        {

            if (_currentTile != null)
                _currentTile.RemoveEntity(this);
            if (destroyGameObject)
                Destroy(gameObject);
        }
    }
}