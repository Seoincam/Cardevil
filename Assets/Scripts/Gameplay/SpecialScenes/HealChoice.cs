using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Cardevil.Gameplay.SpecialScenes
{
    public class HealChoice : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [Header("References")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Image panelImage;
        [SerializeField] private RectTransform innerElementContainer;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private Image iconImage;
        [SerializeField] private TMP_Text valueText;
        [SerializeField] private TMP_Text descriptionText;
        
        [Header("Settings")]
        [SerializeField] private Sprite defaultPanelSprite;
        [SerializeField] private Sprite highlightedPanelSprite;
        [SerializeField] private Sprite goldIconSprite;
        [SerializeField] private Sprite healIconSprite;
        [Header("Hover & Selection")]
        [SerializeField] private float hoverScale = 1.05f;
        [SerializeField] private float animationDuration = 0.2f;
        [Header("Apply")] 
        [SerializeField] private float applyScale = 1.2f;
        [SerializeField] private float applyScaleUpDuration = 0.7f;
        [SerializeField] private float applyHoldDuration = 0.4f;
        [SerializeField] private float applyScaleDownDuration = 0.6f;
        
        
        public event Action Apply;
        public event Action ApplyAnimationCompleted;
        
        private Vector3 _originalScale;
        private Sequence _hoverSequence;
        private Sequence _applySequence;
        private bool _isApplying;
        
        protected void Awake()
        {
            _originalScale = innerElementContainer ? innerElementContainer.localScale : transform.localScale;
        }

        
        public void SetGoldChoice(int goldAmount)
        {
            titleText.text = "골드";
            valueText.text = $"x{goldAmount}";
            descriptionText.text = "체력보다는 돈 \n 체력보다는 돈!";
            iconImage.sprite = goldIconSprite;
            panelImage.sprite = defaultPanelSprite;
            
        }
        
        public void SetHealChoice(int minHealAmount, int maxHealAmount, int defaultHealIndex)
        {
            titleText.text = "회복";
            if (minHealAmount == maxHealAmount)
            {
                valueText.text = $"x{minHealAmount}";
            }
            else
            {
                valueText.text = $"x{minHealAmount}-{maxHealAmount}";
            }
            descriptionText.text = "대결을 잠시 멈추고, \n 충분한 휴식을 취합니다.";
            iconImage.sprite = healIconSprite;
            panelImage.sprite = defaultPanelSprite;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_isApplying)
            {
                return;
            }
            
            Sequence hoverSequence = DOTween.Sequence();
            hoverSequence.Append(innerElementContainer.DOScale(hoverScale, animationDuration).SetEase(Ease.Linear));
            KillHoverSequence();
            _hoverSequence = hoverSequence;
            _hoverSequence.Play();
        }
        
        public void OnPointerExit(PointerEventData eventData)
        {
            if (_isApplying)
            {
                return;
            }

            Sequence exitSequence = DOTween.Sequence();
            exitSequence.Append(innerElementContainer.DOScale(_originalScale, animationDuration).SetEase(Ease.Linear));
            KillHoverSequence();
            _hoverSequence = exitSequence;
            _hoverSequence.Play();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (_isApplying)
            {
                return;
            }

            _isApplying = true;
            panelImage.sprite = highlightedPanelSprite;
            Apply?.Invoke();

            Sequence applySequence = DOTween.Sequence();
            float originalScale = iconImage.transform.localScale.x;
            applySequence.Append(iconImage.transform.DOScale(applyScale, applyScaleUpDuration).SetEase(Ease.OutBack));
            applySequence.AppendInterval(applyHoldDuration);
            applySequence.Append(iconImage.transform.DOScale(originalScale, applyScaleDownDuration)
                .SetEase(Ease.OutBack));
            applySequence.OnComplete(() =>
            {
                ApplyAnimationCompleted?.Invoke();
            });

            KillHoverSequence();
            KillApplySequence();
            _applySequence = applySequence;
            _applySequence.SetTarget(iconImage.gameObject);
            _applySequence.Play();
        }

        private void KillHoverSequence()
        {
            if (_hoverSequence != null && _hoverSequence.IsActive())
            {
                _hoverSequence.Kill();
            }
        }

        private void KillApplySequence()
        {
            if (_applySequence != null && _applySequence.IsActive())
            {
                _applySequence.Kill();
            }
        }
    }
}
