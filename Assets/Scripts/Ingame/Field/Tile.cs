using System.Collections.Generic;
using Cardevil.Attributes;
using Cardevil.Ingame.Entities;
using UnityEngine;

namespace Cardevil.Ingame.Field
{
    /// <summary>
    /// 타일 클래스.
    /// </summary>
    public class Tile : MonoBehaviour
    {
        [Header("Settings")]
        [VisibleOnly] private Vector2Int _coordinate;
        

        [Header("Entities")]
        [SerializeField] private List<Entity> entities = new List<Entity>();
        
        /// <summary>
        /// 타일의 좌표.(Grid 좌표계)
        /// </summary>
        public Vector2Int Coordinate
        {
            get => _coordinate;
            set
            {
                _coordinate = value;
            }
        }
        
    }
}