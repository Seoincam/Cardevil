using Cardevil.Ingame.Field;
using UnityEngine;

namespace Cardevil.Ingame.Entities
{
    public class Entity : MonoBehaviour
    {
        private Tile _currentTile;

        public Tile CurrentTile => _currentTile;
        public Vector2Int Coordinate => _currentTile.Coordinate;


        /// <summary>
        /// 해당 타일로 엔티티를 이동
        /// </summary>
        /// <param name="tile"></param>
        /// <param name="moveTransform">transform까지 이동 여부</param>
        public void MoveTo(Tile tile, bool moveTransform)
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
                transform.position = _currentTile.transform.position;
            }
        }
    }
}