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

namespace Cardevil.Cards.InStage
{
    public class CardVisualUI : MonoBehaviour, IPointerClickHandler, IClearable
    {
        [Header("SO")]
        [SerializeField] private CardVisualSettingSO visualSetting;

        [Header("Visual")] 
        [SerializeField] private CardVisualController visualController;
        [SerializeField] private CardVisualBase visualBase;

        public event Action OnClicked;
        
        private bool _state = true;
        private Tween _backgroundTween, _numberTween;
        
        private readonly Color _darkColor = new(0.2f, 0.2f, 0.2f);

        public void Init(CardData data)
        {
            Clear();
            visualController.Init(data);
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
            
            var color = value ? Color.white : _darkColor;
            // frontBackground.color = color;
            // frontNumber.color = color;
        }

        /// <summary>
        /// <see cref="CardVisualUI"/>의 상태에 따라
        /// UI를 업데이트 함.
        /// </summary>
        public async UniTaskVoid SetStateAsync(bool value, CancellationToken ct = default)
        {
            if (value == _state) return;
            _state = value;
            
            // 이전 트윈 정리
            _backgroundTween?.Kill();
            _numberTween?.Kill();
            
            var color = value ? Color.white : _darkColor;
            float dur = .5f;

            // _backgroundTween = frontBackground
            //     .DOColor(color, dur)
            //     .SetRecyclable(true)
            //     .SetLink(gameObject);
            //
            // _numberTween = frontNumber
            //     .DOColor(color, dur)
            //     .SetRecyclable(true)
            //     .SetLink(gameObject);

            await UniTask.WhenAll(
                _backgroundTween.AwaitForComplete().AttachExternalCancellation(ct),
                _numberTween.AwaitForComplete().AttachExternalCancellation(ct)
            );
        }
    }
}


