using Cardevil.Ingame.Field;
using UnityEngine;

namespace Cardevil.Ingame.Entities
{
    public class Entity : MonoBehaviour
    {
        private Tile _currentTile;

        public Tile CurrentTile => _currentTile;
        public Vector2Int Coordinate => _currentTile.Coordinate;
    }
}