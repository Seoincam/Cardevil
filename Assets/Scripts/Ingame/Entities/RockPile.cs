using Cardevil.Animation;
using Cardevil.Ingame.Entities;
using Cardevil.Pools;
using Cardevil.Utils;
using System;
using TMPro;
using UnityEngine;

namespace Cardevil.Ingame.Entities
{
    [RequireComponent(typeof(Entity), typeof(Poolable))]
    public class RockPile : MonoBehaviour, IReflectorEntity, IPoolableSubComponent
    {
        [SerializeField] private Entity entity;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Animator animator;
        [SerializeField] private AnimSignalListener animSignalListener;
        [SerializeField] private TMP_Text countText;
        [SerializeField] private Poolable _poolable;
        
        public Entity Entity => entity;
        public Poolable Poolable => _poolable;
        
        private void Reset()
        {
            entity = GetComponent<Entity>();
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            animSignalListener = GetComponentInChildren<AnimSignalListener>();
            countText = GetComponentInChildren<TMP_Text>();
            _poolable = GetComponent<Poolable>();
        }
        

        public bool DoReflect => _rockCount > 0;
        
        [SerializeField] private int _rockCount = 3;
        
        public int RockCount
        {
            get => _rockCount;
            set
            {
                _rockCount = value;
                if (countText != null)
                {
                    countText.text = _rockCount.ToString();
                }
            }
        }

        private void OnEnable()
        {
            animSignalListener.SignalEvent += OnAnimSignal;
        }
        
        public void Init(int initialRockCount)
        {
            RockCount = initialRockCount;
            animator.ResetTrigger(AnimatorHashes.Break);
        }

        private void OnDisable()
        {
            animSignalListener.SignalEvent -= OnAnimSignal;
        }
        
        private void OnAnimSignal(string signal)
        {
            if (signal == "BreakEnd")
            {
                entity.Kill(false);
                _poolable.Release();
            }
        }

        public void OnReflect()
        {
            RockCount--;
            if (RockCount <= 0)
            {
                animator.SetTrigger(AnimatorHashes.Break);
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.gray;
            // 남은 수 표시
            UnityEditor.Handles.Label(transform.position + Vector3.up * 1.5f, _rockCount.ToString());  
        }
    }

    public interface IReflectorEntity : IEntityComponent
    {
        public bool DoReflect { get;  }

        public void OnReflect();
    }
}