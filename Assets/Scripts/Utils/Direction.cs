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

    public enum RareType
    {
        Noraml,
        Rare,
        Epic,
        Legend
    }

    [Flags]
    public enum DirectionFlag
    {
        None = 0,
        Up = 1 << 0,
        Right = 1 << 1,
        Down = 1 << 2,
        Left = 1 << 3,
        
        UpDown = Up | Down,
        LeftRight = Left | Right,
        All = Up | Right | Down | Left
    }
    
    public static class DirectionExtensions
    {
        public static TileVector ToTileVector(this Direction direction)
        {
            return direction switch
            {
                Direction.None => TileVector.Zero,
                Direction.Up => TileVector.Up,
                Direction.Right => TileVector.Right,
                Direction.Down => TileVector.Down,
                Direction.Left => TileVector.Left,
                _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
            };
        }

        public static Vector2Int ToVector2Int(this Direction direction)
        {
            return direction.ToTileVector().ToVector2Int();
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
        
        public static Direction Opposite(this Direction direction)
        {
            return direction switch
            {
                Direction.Up => Direction.Down,
                Direction.Right => Direction.Left,
                Direction.Down => Direction.Up,
                Direction.Left => Direction.Right,
                _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
            };
        }
        
        public static Direction Clockwise(this Direction direction)
        {
            return direction switch
            {
                Direction.Up => Direction.Right,
                Direction.Right => Direction.Down,
                Direction.Down => Direction.Left,
                Direction.Left => Direction.Up,
                _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
            };
        }
        
        public static Direction CounterClockwise(this Direction direction)
        {
            return direction switch
            {
                Direction.Up => Direction.Left,
                Direction.Right => Direction.Up,
                Direction.Down => Direction.Right,
                Direction.Left => Direction.Down,
                _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
            };
        }
        

        
    }
}