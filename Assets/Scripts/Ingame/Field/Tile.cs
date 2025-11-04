using System;
using System.Collections.Generic;
using Cardevil.Attributes;
using Cardevil.Ingame.Entities;
using Cardevil.Pools;
using Cardevil.Utils;
using UnityEngine;
using UnityEngine.Serialization;

namespace Cardevil.Ingame.Field
{
    /// <summary>
    /// 타일 클래스.
    /// </summary>
    public class Tile : MonoBehaviour
    {
        [Header("Settings")]
        [VisibleOnly, SerializeField] private Field _field;
        [VisibleOnly, SerializeField] private FieldConfigurationSO _fieldConfiguration;
        [VisibleOnly(EditableIn.EditMode), SerializeField] private TileVector _coordinate;
        
        [Header("Internal References")]
        [VisibleOnly(EditableIn.EditMode), SerializeField] private SpriteRenderer _spriteRenderer;
        [VisibleOnly(EditableIn.EditMode), SerializeField] private MeshRenderer _meshRenderer;
        [VisibleOnly(EditableIn.EditMode), SerializeField] private Collider _collider;

        [Header("Entities")]
        [VisibleOnly,SerializeField] private List<Entity> entities = new List<Entity>();
        
        [Header("Objects")]
        [VisibleOnly, SerializeField] private List<TileHighlight> _highlightObjects = new List<TileHighlight>();

        public Field Field => _field;
        
        /// <summary>
        /// 타일의 좌표.(Grid 좌표계)
        /// </summary>
        public TileVector Coordinate
        {
            get => _coordinate;
        }

        public void Initialize(Field field, TileVector coordinate)
        {
            this._coordinate = coordinate;
            _field = field;
            _fieldConfiguration = field.FieldConfiguration;
            
        }

        public bool AddEntity(Entity entity){
            if (entity == null)
            {
                LogEx.LogError("Cannot add a null entity to the tile.");
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
                LogEx.LogError("Cannot remove a null entity from the tile.");
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
        
        [ContextMenu("Highlight Tile")]
        [Obsolete("Use Highlight(Define.HighlightType) instead.")]
        public void HighLightAttackTile() // 공격받을 타일의 하이라이트 위치
        { 
            // SetColor(_fieldConfiguration.DefaultTileHighlightColor);
            Highlight(Define.HighlightType.DefaultAttack);
        }
        
        [ContextMenu("UnHighlight Tile")]
        [Obsolete("Use Unhightlight(Define.HighlightType, bool) instead.")]
        public void UnHighLightAttackTile() // 공격받을 타일의 하이라이트 제거
        {
            // SetColor(_fieldConfiguration.DefaultTileColor);
            Unhightlight(Define.HighlightType.DefaultAttack);
        }


        public TileHighlight Highlight(Define.HighlightType highlightType)
        {
            TileHighlight highlightObject = Managers.Pool.Get<TileHighlight>(Poolables.TileHighlight);
            if (highlightObject == null)
            {
                Debug.LogError("Failed to get TileHighlight from pool.");
                return null;
            }
            highlightObject.Initialize(this, _fieldConfiguration);
            highlightObject.SetHighlightColor(highlightType, _fieldConfiguration.DefaultTileHighlightColor);
            highlightObject.transform.SetParent(transform, false);
            highlightObject.transform.position = transform.position;
            _highlightObjects.Add(highlightObject);
            return highlightObject;
        }
        
        public void Unhightlight(Define.HighlightType highlightType, bool removeAll = false)
        {
            if (_highlightObjects == null || _highlightObjects.Count == 0)
            {
                Debug.LogWarning("No highlight objects to remove.");
                return;
            }

            for (int i = _highlightObjects.Count - 1; i >= 0; i--)
            {
                if (_highlightObjects[i] != null && _highlightObjects[i].HighlightType == highlightType)
                {
                    Managers.Pool.Release(_highlightObjects[i].Poolable);
                    _highlightObjects.RemoveAt(i);
                    if (removeAll == false)
                    {
                        break; // removeAll이 false인 경우 첫 번째 일치하는 하이라이트만 제거하고 종료
                    }
                }
            }
        }

        public void SetColor(Color color)
        {
            if (_spriteRenderer != null)
            {
                _spriteRenderer.color = color;
            }
            if (_meshRenderer != null)
            {
                MaterialPropertyBlock block = new MaterialPropertyBlock();
                _meshRenderer.GetPropertyBlock(block);
                block.SetColor(ShaderHashes.SHADER_COLOR, color);
            }
        }
        
        
    }
}