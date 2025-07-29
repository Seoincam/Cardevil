using System.Collections;
using System.Collections.Generic;
using Cardevil.Utils.Directions;
using UnityEngine;

namespace Ingame.Field
{
    /// <summary>
    /// 보드판.
    /// </summary>
    /// <remarks>
    /// 일종의 타일 컨테이너 클래스.
    /// 음수인덱스는 지원 안함. 필요시 지원 예정
    /// </remarks>
    [RequireComponent(typeof(Grid))]
    public class Field : MonoBehaviour, IEnumerable<Tile>, IGridTileContainer
    {
        
        private Tile[][] tileContainer;
        [SerializeField] private Grid grid;
        public Grid Grid => grid;
        
        [SerializeField, VisibleOnly(EditableIn.EditMode)] int width = 3;
        [SerializeField, VisibleOnly(EditableIn.EditMode)] int height = 3;
        
        public void InitField(int width, int height)
        {
            this.width = width;
            this.height = height;
            tileContainer = new Tile[height][];
            for (int y = 0; y < height; y++)
            {
                tileContainer[y] = new Tile[width];
                for (int x = 0; x < width; x++)
                {
                    // TODO : 타일 초기화 로직
                }
            }
        }
        
        public Vector3 GetCenterPosition()
        {
            var LeftBottom = grid.GetCellCenterWorld(new Vector3Int(0, 0, 0));
            var RightTop = grid.GetCellCenterWorld(new Vector3Int(width - 1, height - 1, 0));
            return (LeftBottom + RightTop) / 2;
        }
        
        public Tile WorldToTile(Vector3 worldPosition)
        {
            var localPosition = transform.InverseTransformPoint(worldPosition);
            var cellPosition = grid.WorldToCell(localPosition);
            if (cellPosition.x < 0 || cellPosition.x >= width || cellPosition.y < 0 || cellPosition.y >= height)
                return null;
            return tileContainer[cellPosition.y][cellPosition.x];
        }
        public Vector2Int WorldToCoordinate(Vector3 worldPosition)
        {
            var localPosition = transform.InverseTransformPoint(worldPosition);
            var cellPosition = grid.WorldToCell(localPosition);
            return new Vector2Int(cellPosition.x, cellPosition.y);
        }
        
        public Tile GetTile(Vector2Int coordinate)
        {
            return GetTile(coordinate.x, coordinate.y);
        }
        public Tile GetTile(int x, int y)
        {
            if (x < 0 || x >= width || y < 0 || y >= height)
                return null;
            return tileContainer[y][x];
        }
        public Tile GetTileByDirection(Tile tile, Direction direction)
        {
            var coordinate = tile.Coordinate;
            var nextCoordinate = coordinate + direction.ToVector2Int();
            return GetTile(nextCoordinate);
        }
        public Vector3 GetTilePosition(Vector2Int coordinate)
        {
            return GetTilePosition(coordinate.x, coordinate.y);
        }
        public Vector3 GetTilePosition(int x, int y)
        {
            return tileContainer[y][x].transform.position;
        }
        
        public Tile[] this[int i]
        {
            get
            {
                if (i < 0 || i >= height)
                    return null;
                return tileContainer[i];
            }
        }
        
        
        #if UNITY_EDITOR
        [ContextMenu("Init Field Editor")]
        public void InitFieldEditor()
        {
            width = 3;
            height = 3;
            InitField(width, height);
        }
        #endif
        
        public IEnumerator<Tile> GetEnumerator()
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    yield return tileContainer[y][x];
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}