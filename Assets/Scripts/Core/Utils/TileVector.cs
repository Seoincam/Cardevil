using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Cardevil.Utils
{
    [Serializable]
    public struct TileVector
    {
        public static readonly TileVector Zero = new TileVector(0, 0);
        public static readonly TileVector One = new TileVector(1, 1);
        public static readonly TileVector Up = new TileVector(1, 0);
        public static readonly TileVector Right = new TileVector(0, -1);
        public static readonly TileVector Down = new TileVector(-1, 0);
        public static readonly TileVector Left = new TileVector(0, 1);
        
        [SerializeField] private int _i;
        [SerializeField] private int _j;
        
        // ReSharper disable InconsistentNaming
        public int i
        {
            get => _i;
            set => _i = value;
        }
        public int j
        {
            get => _j;
            set => _j = value;
        }
        
        public TileVector(int i = 0, int j = 0)
        {
            this._i = i;
            this._j = j;
        }
        
        public Vector2Int ToVector2Int()
        {
            return new Vector2Int(_j,_i);
        }
        
        public Vector2Int ToVector2IntDirect()
        {
            return new Vector2Int(_i,_j);
        }
        
        public static TileVector FromVector2Int(Vector2Int vector)
        {
            return new TileVector(vector.y, vector.x);
        }
        
        public static TileVector FromVector2IntDirect(Vector2Int vector)
        {
            return new TileVector(vector.x, vector.y);
        }
        
        
        public override string ToString()
        {
            return $"({_i}, {_j})";
        }
        
        public static TileVector operator +(TileVector a, TileVector b)
        {
            return new TileVector(a.i + b.i, a.j + b.j);
        }
        public static TileVector operator -(TileVector a, TileVector b)
        {
            return new TileVector(a.i - b.i, a.j - b.j);
        }
        public static TileVector operator *(TileVector a, int scalar)
        {
            return new TileVector(a.i * scalar, a.j * scalar);
        }
        public static TileVector operator *(int scalar, TileVector a)
        {
            return new TileVector(a.i * scalar, a.j * scalar);
        }
        public static TileVector operator /(TileVector a, int scalar)
        {
            if (scalar == 0) throw new DivideByZeroException("Cannot divide by zero.");
            return new TileVector(a.i / scalar, a.j / scalar);
        }
        public static bool operator ==(TileVector a, TileVector b)
        {
            return a.i == b.i && a.j == b.j;
        }
        public static bool operator !=(TileVector a, TileVector b)
        {
            return !(a == b);
        }
        public override bool Equals(object obj)
        {
            if (obj is TileVector vector)
            {
                return this == vector;
            }
            return false;
        }
        public override int GetHashCode()
        {
            unchecked
            {
                return (i * 397) ^ j;
            }
        }
        
    }
}