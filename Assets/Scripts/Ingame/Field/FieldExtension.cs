using Cardevil.Ingame.Entities;
using Cardevil.Utils;
using UnityEngine;

namespace Cardevil.Ingame.Field
{
    public static class FieldExtension
    {
        
        public static Entity SummonEntity(this Field field, Entity entityPrefab, TileVector coordinate)
        {
            Tile tile = field.GetTile(coordinate);
            if (tile == null)
            {
                LogEx.LogError($"Cannot summon entity at invalid coordinate {coordinate}.");
                return null;
            }
            return field.SummonEntity(entityPrefab, tile);
        } 
        
        public static Entity SummonEntity(this Field field, Entity entityPrefab, Tile tile)
        {
            Entity entityInstance = Object.Instantiate(entityPrefab);
            entityInstance.Init(tile);
            return entityInstance;
        }
        
        public static T SummonEntityComponent<T>(this Field field, T entityComponent, TileVector coordinate) where T : MonoBehaviour, IEntityComponent
        {
            Tile tile = field.GetTile(coordinate);
            if (tile == null)
            {
                LogEx.LogError($"Cannot summon entity component at invalid coordinate {coordinate}.");
                return null;
            }
            return field.SummonEntityComponent(entityComponent, tile);
        }
        
        public static T SummonEntityComponent<T>(this Field field, T entityComponent, Tile tile) where T : MonoBehaviour, IEntityComponent
        {
            if (entityComponent == null)
            {
                LogEx.LogError($"Cannot summon null entity component.");
                return null;
            }
            
            Entity entity = entityComponent.Entity;
            if (entity == null)
            {
                LogEx.LogError($"Entity component does not have a valid Entity reference.");
                return null;
            }
            
            entity.Init(tile);
            return entityComponent;
        }
    }
}