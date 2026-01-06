using Cardevil.Ingame.Entities;
using System;
using TMPro;
using UnityEngine;

namespace Cardevil.Ingame
{
    [RequireComponent(typeof(SpriteRenderer), typeof(Entity))]
    public class RockPile : MonoBehaviour, IReflecterEntity
    {
        [SerializeField] private Entity entity;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private TMP_Text countText;

        private void Reset()
        {
            entity = GetComponent<Entity>();
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        public bool DoReflect => _rockCount > 0;
        
        private int _rockCount;
        
        public int RockCount
        {
            get => _rockCount;
            set
            {
                _rockCount = value;
                countText.text = _rockCount.ToString();
            }
        }
        
        public void OnReflect()
        {
            RockCount--;
            if (RockCount <= 0)
            {
                entity.Kill();
            }
        }
    }

    public interface IReflecterEntity
    {
        public bool DoReflect { get;  }

        public void OnReflect();
    }
}