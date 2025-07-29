using System;
using UnityEngine;

namespace Cardevil.Utils.Directions
{
    public enum Direction
    {
        None = -1,
        Up,
        Right,
        Down,
        Left,
    }

    [Flags]
    public enum DirectionFlag
    {
        None = 0,
        Up = 1 << 0,
        Right = 1 << 1,
        Down = 1 << 2,
        Left = 1 << 3,
        All = Up | Right | Down | Left
    }
    
    public static class DirectionExtensions
    {
        public static readonly Vector2Int UpVector = new Vector2Int(0, 1);
        public static readonly Vector2Int RightVector = new Vector2Int(1, 0);
        public static readonly Vector2Int DownVector = new Vector2Int(0, -1);
        public static readonly Vector2Int LeftVector = new Vector2Int(-1, 0);
        
        public static Vector2Int ToVector2Int(this Direction direction)
        {
            return direction switch
            {
                Direction.Up => UpVector,
                Direction.Right => RightVector,
                Direction.Down => DownVector,
                Direction.Left => LeftVector,
                _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
            };
        }
        
        public static DirectionFlag ToDirectionFlag(this Direction direction)
        {
            return direction switch
            {
                Direction.None => DirectionFlag.None,
                Direction.Up => DirectionFlag.Up,
                Direction.Right => DirectionFlag.Right,
                Direction.Down => DirectionFlag.Down,
                Direction.Left => DirectionFlag.Left,
                _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
            };
        }
    }
}