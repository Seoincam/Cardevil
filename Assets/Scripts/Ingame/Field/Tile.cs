using System;
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

        [VisibleOnly, SerializeField] private Field _field;
        [VisibleOnly, SerializeField] private Vector2Int _coordinate;

        [Header("Entities")]
        [SerializeField] private List<Entity> entities = new List<Entity>();

        public Field Field => _field;
        
        /// <summary>
        /// 타일의 좌표.(Grid 좌표계)
        /// </summary>
        public Vector2Int Coordinate
        {
            get => _coordinate;
        }

        public void Initialize(Field field, Vector2Int coordinate)
        {
            _coordinate = coordinate;
            transform.position = field.Grid.GetCellCenterWorld(new Vector3Int(coordinate.x, coordinate.y, 0));
        }

        public bool AddEntity(Entity entity){
            if (entity == null)
            {
                Debug.LogError("Cannot add a null entity to the tile.");
                return false;
            }
            
            if (!entities.Contains(entity))
            {
                entities.Add(entity);
                return true;
            }
            return false;
        }
        
        public bool RemoveEntity(Entity entity)
        {
            if (entity == null)
            {
                Debug.LogError("Cannot remove a null entity from the tile.");
                return false;
            }
            
            if (entities.Contains(entity))
            {
                entities.Remove(entity);
                return true;
            }
            return false;
        }
        public List<Entity> GetEntities()
        {
            return entities;
        }
        
        public bool HasEntity() => entities.Count > 0;
        public bool HasEntity(Entity entity)
        {
            return entities.Contains(entity);
        }
        public bool HasEntity(Predicate<Entity> predicate)
        {
            return entities.Exists(predicate);
        }
        public bool TryGetFirstEntity(Predicate<Entity> predicate, out Entity entity)
        {
            entity = entities.Find(predicate);
            return entity != null;
        }
        
        public void GetEntities(Predicate<Entity> predicate, ref List<Entity> result)
        {
            if (result == null)
            {
                Debug.LogError("Result list cannot be null.");
                return;
            }
            result.Clear();
            foreach (var entity in entities)
            {
                if (predicate(entity))
                {
                    result.Add(entity);
                }
            }
        }
        
        public void HighLightAttackTile() // 공격받을 타일의 하이라이트 위치
        { 

        }
    }
}