using System;
using System.Collections;
using System.Collections.Generic;
using Cardevil.Attributes;
using Cardevil.Utils;
using Cardevil.Utils.Directions;
using UnityEngine;

namespace Cardevil.Ingame.Field
{
    /// <summary>
    /// 보드판.
    /// </summary>
    /// <remarks>
    /// 일종의 타일 컨테이너 클래스.
    /// 음수인덱스는 지원 안함. 필요시 지원 예정
    /// Tile [세로값][가로값] 값은 0,1,2 값
    /// </remarks>
   
    [RequireComponent(typeof(Grid))]
    public class Field : MonoBehaviour, IEnumerable<Tile>, IGridTileContainer
    {
        [Header("Settings")]
        [SerializeField, VisibleOnly(EditableIn.EditMode)] FieldConfigurationSO fieldConfiguration;
        [SerializeField, VisibleOnly(EditableIn.EditMode)] int width = 3;
        [SerializeField, VisibleOnly(EditableIn.EditMode)] int height = 3;
        [SerializeField, VisibleOnly(EditableIn.EditMode)] Tile _tilePrefab;
        [SerializeField] private bool _initOnAwake = true;
        [Header("References")]
        [SerializeField] private Grid grid;
        [SerializeField, VisibleOnly] private TileLine[] _tileContainer;

        [Serializable]
        internal class TileLine : IEnumerable<Tile>
        {
            [SerializeField] private Tile[] tiles;
            
            
            internal TileLine(int size = 3)
            {
                tiles = new Tile[size];
            }

            public Tile this[int index]
            {
                get
                {
                    if (index < 0 || index >= tiles.Length)
                        throw new IndexOutOfRangeException($"Index {index} is out of range for TileLine.");
                    return tiles[index];
                }
                set
                {
                    if (index < 0 || index >= tiles.Length)
                        throw new IndexOutOfRangeException($"Index {index} is out of range for TileLine.");
                    tiles[index] = value;
                }
            }

            public int Length => tiles.Length;
            public IEnumerator<Tile> GetEnumerator()
            {
                for (int i = 0; i < tiles.Length; i++)
                {
                    yield return tiles[i];
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public static implicit operator Tile[](TileLine line)
            {
                return line.tiles;
            }
        }
        
        
        public FieldConfigurationSO FieldConfiguration => fieldConfiguration;
        public Grid Grid => grid;


        private void Awake()
        {
            Managers.Game.field = this; // 시작될때 매니저에 등록
            if (fieldConfiguration == null)
            {
                Debug.LogError("FieldConfigurationSo is not assigned. Please assign it in the inspector.");
                return;
            }

            if (_initOnAwake)
            {
                InitField(3,3);
            }
        }

        [ContextMenu("Init Field")]
        public void InitField(int width, int height)
        {
            ClearTiles();
            this.width = width;
            this.height = height;
            _tileContainer = new TileLine[height];
            for (int i = 0; i < height; i++)
            {
                _tileContainer[i] = new TileLine();
                for (int j = 0; j < width; j++)
                {
                    Tile tile = Instantiate(_tilePrefab, transform);
                    tile.Initialize(this, new TileVector(i, j));
                    tile.name = $"Tile_({i},{j})";
                    tile.transform.position = Grid.GetCellCenterWorld(new Vector3Int(j,i, 0));
                    tile.transform.localScale = Vector3.one * fieldConfiguration.TileSize;
                    _tileContainer[i][j] = tile;
                }
            }
        }

        [ContextMenu("Clear Tiles")]
        public void ClearTiles()
        {
            if (_tileContainer == null)
                return;

            foreach (var row in _tileContainer)
            {
                foreach (var tile in row)
                {
                    if (tile != null)
                    {
                        #if UNITY_EDITOR
                        if (Application.isPlaying)
                        {
                            Destroy(tile.gameObject);
                        }
                        else
                        {
                            DestroyImmediate(tile.gameObject);
                        }
                        #else
                        Destroy(tile.gameObject);  
                        #endif  
                    }
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
            return _tileContainer[cellPosition.y][cellPosition.x];
        }
        public TileVector WorldToCoordinate(Vector3 worldPosition)
        {
            var localPosition = transform.InverseTransformPoint(worldPosition);
            var cellPosition = grid.WorldToCell(localPosition);
            return new TileVector(cellPosition.y, cellPosition.x);
        }
        
        public Tile GetTile(TileVector tile)
        {
            return GetTile(tile.i, tile.j);
        }
        public Tile GetTile(int i, int j)
        {
            if (i < 0 || i >= height || j < 0 || j >= width)
            {
                Debug.LogError($"GetTile: Index out of range. ({i}, {j}) is not a valid tile coordinate.");
                return null;
            }
            return _tileContainer[i][j];
        }
        public Tile GetTileByDirection(Tile tile, Direction direction, bool wrapAround = false)
        {
            var coordinate = tile.Coordinate;
            var nextCoordinate = coordinate + direction.ToCoordinateVector();
            if (wrapAround)
            {
                while (nextCoordinate.i < 0)
                    nextCoordinate.i += height;
                while (nextCoordinate.i >= height)
                    nextCoordinate.i -= height;

                while (nextCoordinate.j < 0)
                    nextCoordinate.j += width;
                while (nextCoordinate.j >= width)
                    nextCoordinate.j -= width;
            }
            return GetTile(nextCoordinate);
        }
        

        public Tile GetTileByDelta(Tile tile, TileVector delta, bool wrapAround = false)
        {
            var coordinate = tile.Coordinate;
            var nextCoordinate = coordinate + delta;
            if (wrapAround)
            {
                while (nextCoordinate.i < 0)
                    nextCoordinate.i += height;
                while (nextCoordinate.i >= height)
                    nextCoordinate.i -= height;

                while (nextCoordinate.j < 0)
                    nextCoordinate.j += width;
                while (nextCoordinate.j >= width)
                    nextCoordinate.j -= width;
            }
            return GetTile(nextCoordinate);
        }
        
        
        public Vector3 GetTilePosition(TileVector tile)
        {
            return GetTilePosition(tile.i, tile.j);
        }
        public Vector3 GetTilePosition(int i, int j)
        {
            if (i < 0 || i >= height || j < 0 || j >= width)
            {
                Debug.LogError($"GetTilePosition: Index out of range. ({i}, {j}) is not a valid tile coordinate.");
                return Vector3.zero;
            }
            return _tileContainer[i][j].transform.position;
        }

        public List<Tile> GetHorizontalTiles(int i)
        {
            // TODO : 캐싱
            if (i < 0 || i >= height)
                return null;
            List<Tile> horizontalTiles = new List<Tile>();
            for (int x = 0; x < width; x++)
            {
                horizontalTiles.Add(_tileContainer[i][x]);
            }
            return horizontalTiles;
        }
        
        public List<Tile> GetVerticalTiles(int j)
        {
            // TODO : 캐싱
            if (j < 0 || j >= width)
                return null;
            List<Tile> verticalTiles = new List<Tile>();
            for (int y = 0; y < height; y++)
            {
                verticalTiles.Add(_tileContainer[y][j]);
            }
            return verticalTiles;
        }

        public List<Tile> GetRectangleTiles(int si, int sj, int ei, int ej)
        {
            si = Mathf.Clamp(si, 0, height - 1);
            sj = Mathf.Clamp(sj, 0, width - 1);
            ei = Mathf.Clamp(ei, 0, height - 1);
            ej = Mathf.Clamp(ej, 0, width - 1);
            List<Tile> rectangleTiles = new List<Tile>();
            for (int i = Mathf.Min(si, ei); i <= Mathf.Max(si, ei); i++)
            {
                for (int j = Mathf.Min(sj, ej); j <= Mathf.Max(sj, ej); j++)
                {
                    rectangleTiles.Add(_tileContainer[i][j]);
                }
            }
            return rectangleTiles;
        }

        public List<Tile> GetRectangleTiles(TileVector start, TileVector end)
        {
            return GetRectangleTiles(start.i, start.j, end.i, end.j);
        }
        
        
        public Tile[] this[int i]
        {
            get
            {
                if (i < 0 || i >= height)
                    return null;
                return _tileContainer[i];
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
                    yield return _tileContainer[y][x];
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator(); 
        }
    }
}