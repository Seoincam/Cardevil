using Cardevil.Ingame.Entities;
using Cardevil.Utils;
using Cysharp.Threading.Tasks;
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
        
        public static T SummonEntityComponent<T>(this Field field, T entityComponentPrefab, TileVector coordinate) where T : MonoBehaviour, IEntityComponent
        {
            Tile tile = field.GetTile(coordinate);
            if (tile == null)
            {
                LogEx.LogError($"Cannot summon entity component at invalid coordinate {coordinate}.");
                return null;
            }
            return field.SummonEntityComponent(entityComponentPrefab, tile);
        }
        
        public static T SummonEntityComponent<T>(this Field field, T entityComponentPrefab, Tile tile) where T : MonoBehaviour, IEntityComponent
        {
            Entity entityInstance = Object.Instantiate(entityComponentPrefab.Entity);
            entityInstance.Init(tile);
            T componentInstance = entityInstance.GetComponent<T>();
            if (componentInstance == null)
            {
                LogEx.LogError($"The summoned entity does not have the required component of type {typeof(T).Name}.");
                Object.Destroy(entityInstance.gameObject);
                return null;
            }
            return componentInstance;
        }
    }
}