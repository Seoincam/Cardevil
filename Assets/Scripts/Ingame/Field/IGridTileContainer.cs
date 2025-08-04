using Cardevil.Utils.Directions;
using UnityEngine;

namespace Cardevil.Ingame.Field
{
    /// <summary>
    /// 2D 타일 기반 타일 컨테이너 인터페이스.
    /// </summary>
    public interface IGridTileContainer
    {
        
        /// <summary>
        /// 월드 좌표를 타일로 변환.
        /// </summary>
        /// <param name="worldPosition"></param>
        /// <returns></returns>
        Tile WorldToTile(Vector3 worldPosition);
        
        /// <summary>
        /// 월드 좌표계를 타일 좌표계로 변환.
        /// </summary>
        /// <param name="worldPosition"></param>
        /// <returns></returns>
        Vector2Int WorldToCoordinate(Vector3 worldPosition);
        
        /// <summary>
        /// 타일 좌표계를 월드 좌표계로 변환.
        /// </summary>
        /// <returns></returns>
        Vector3 GetCenterPosition();
        
        /// <summary>
        /// 타일 좌표를 받아와 타일을 반환한다.
        /// </summary>
        /// <param name="coordinate"></param>
        /// <returns></returns>
        Tile GetTile(Vector2Int coordinate);
        
        /// <summary>
        /// 타일 좌표를 받아와 타일을 반환한다.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        /// <seealso cref="this"/>
        Tile GetTile(int x, int y);
        
        /// <summary>
        /// 타일을 기준으로 방향에 따라 타일을 반환한다.
        /// </summary>
        /// <param name="tile"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        Tile GetTileByDirection(Tile tile, Direction direction);
        
        
        /// <summary>
        /// 타일 좌표를 받아와 타일의 월드 좌표를 반환한다.
        /// </summary>
        /// <param name="coordinate"></param>
        /// <returns></returns>
        Vector3 GetTilePosition(Vector2Int coordinate);
        
        /// <summary>
        /// 타일 좌표를 받아와 타일의 월드 좌표를 반환한다.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        Vector3 GetTilePosition(int x, int y);
        
        /// <summary>
        /// 타일 컨테이너의 행을 인덱스로 받아와 해당 행의 타일 배열을 반환한다.
        /// </summary>
        /// <param name="i"></param>
        Tile[] this[int i] { get; }
        
    }
}