using UnityEngine;
using Cardevil.Cards.Data.InStage;
using Cardevil.Cards.ScriptableObjects;
using Cardevil.Cards.Visual;
using Cardevil.Core;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Threading;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Cardevil.Cards.InStage
{
    [RequireComponent(typeof(CanvasGroup))]
    public class CardVisualUI : MonoBehaviour, IPointerClickHandler, IClearable
    {
        [Header("SO")]
        [SerializeField] private CardVisualSettingSO visualSetting;

        [Header("Visual")] 
        [SerializeField] private CardVisualController visualController;
        [SerializeField] private CardVisualBase visualBase;
        [SerializeField] private Image cover; 

        public event Action OnClicked;
        public CanvasGroup CanvasGroup { get; private set; }
        public RectTransform Rect { get; private set; }
        
        private bool _state = true;
        private Tween _coverTween;

        private readonly Color _noColor = new(0, 0, 0, 0);
        private readonly Color _darkColor = new(0, 0, 0, .8f);

        private void Awake()
        {
            CanvasGroup = GetComponent<CanvasGroup>();
            Rect = GetComponent<RectTransform>();
        }

        public void Init(CardData data)
        {
            Clear();
            visualController.Init(data);
            SetStateImmediate(true);
            
        }
        
        public void Clear()
        {
            OnClicked = null;
            _state = true;
            SetStateImmediate(_state);
        }
        
        public void UpdateVisual(CardData data)
        {
            visualController.UpdateData(data).Forget();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            OnClicked?.Invoke();
        }

        /// <summary>
        /// <see cref="CardVisualUI"/>의 상태에 따라
        /// UI를 즉각적으로 업데이트 함.
        /// </summary>
        public void SetStateImmediate(bool value)
        {
            if (value == _state) return;
            _state = value;

            cover.color = value ? _noColor : _darkColor;
        }

        /// <summary>
        /// <see cref="CardVisualUI"/>의 상태에 따라
        /// UI를 업데이트 함.
        /// </summary>
        public async UniTaskVoid SetStateAsync(bool value, CancellationToken ct = default)
        {
            if (value == _state) return;
            _state = value;
            
            _coverTween?.Kill();
            float dur = .5f;

            await cover.DOColor(value ? _noColor : _darkColor, dur)
                .SetRecyclable(true)
                .SetLink(gameObject)
                .WithCancellation(ct);
        }
    }
}


