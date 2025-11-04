using Cardevil.Utils;
using System.Collections.Generic;
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
        TileVector WorldToCoordinate(Vector3 worldPosition);
        
        /// <summary>
        /// 타일 좌표계를 월드 좌표계로 변환.
        /// </summary>
        /// <returns></returns>
        Vector3 GetCenterPosition();
        
        /// <summary>
        /// 타일 좌표를 받아와 타일을 반환한다.
        /// </summary>
        /// <param name="tiledinate"></param>
        /// <returns></returns>
        Tile GetTile(TileVector tile);
        
        /// <summary>
        /// 타일 좌표를 받아와 타일을 반환한다.
        /// </summary>
        /// <param name="i"></param>
        /// <param name="j"></param>
        /// <returns></returns>
        /// <seealso cref="this"/>
        Tile GetTile(int i, int j);
        
        /// <summary>
        /// 타일을 기준으로 방향에 따라 타일을 반환한다.
        /// </summary>
        /// <param name="tile"></param>
        /// <param name="direction"></param>
        /// <param name="wrapAround">타일 컨테이너의 경계를 넘어갈 경우, 반대편 타일을 반환할지 여부</param>
        /// <returns></returns>
        Tile GetTileByDirection(Tile tile, Direction direction, bool wrapAround = false, int distance = 1);
        
        /// <summary>
        /// 타일을 기준으로 방향에 따라 타일을 반환한다.
        /// </summary>
        /// <param name="tile"></param>
        /// <param name="direction"></param>
        /// <param name="wrapped"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        Tile GetTileByDirectionWrap(Tile tile, Direction direction, out bool wrapped, int distance = 1);
        
        
        /// <summary>
        /// 타일 좌표를 받아와 타일의 월드 좌표를 반환한다.
        /// </summary>
        /// <param name="tiledinate"></param>
        /// <returns></returns>
        Vector3 GetTilePosition(TileVector tile);
        
        /// <summary>
        /// 타일 좌표를 받아와 타일의 월드 좌표를 반환한다.
        /// </summary>
        /// <param name="i"></param>
        /// <param name="j"></param>
        /// <returns></returns>
        Vector3 GetTilePosition(int i, int j);


        /// <summary>
        /// 해당 행의 타일들을 반환
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public List<Tile> GetHorizontalTiles(int i);

        /// <summary>
        /// 해당 열의 타일들을 반환
        /// </summary>
        /// <param name="j"></param>
        /// <returns></returns>
        public List<Tile> GetVerticalTiles(int j);

        public List<Tile> GetRectangleTiles(int si,int sj, int ei, int ej)
        {
            return GetRectangleTiles(new TileVector(si, sj), new TileVector(ei, ej));
        }
        /// <summary>
        /// 타일 컨테이너의 직사각형 영역에 해당하는 타일들을 반환한다.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public List<Tile> GetRectangleTiles(TileVector start, TileVector end);
        
        /// <summary>
        /// 타일 컨테이너의 행을 인덱스로 받아와 해당 행의 타일 배열을 반환한다.
        /// </summary>
        /// <param name="i"></param>
        Tile[] this[int i] { get; }
        
    }
}