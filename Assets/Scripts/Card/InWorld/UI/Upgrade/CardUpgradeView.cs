using Cardevil.Card.Common;
using Cardevil.Card.Common.Core.Upgrade;
using Cardevil.Card.Common.Visual;
using Cardevil.Card.Visual.Controller;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Cardevil.Card.InWorld.UI.Upgrade
{
    public class CardUpgradeView : MonoBehaviour
    {
        public event Action<UpgradeNodeSO> SelectedNodeChanged;
        public event Action<UpgradeNodeSO> ConfirmClicked;
        public event Action CloseClicked;
        
        
        [Header("Prefabs")]
        [SerializeField] private InteractionCard cardPrefab;
        
        [Header("Settings")]
        [SerializeField] private float fadeDuration = 1f;
        
        [Header("Scene References")]
        [SerializeField] private Transform originalAnchor;
        [SerializeField] private Transform next_1_0Anchor;
        [SerializeField] private Transform next_2_0Anchor;
        [SerializeField] private Transform next_2_1Anchor;
        
        [Space, SerializeField] private CanvasGroup canvasGroup;

        [Space, SerializeField] private Button confirmButton;
        [SerializeField] private Button cancelButton;
        [SerializeField] private TextMeshProUGUI costText;


        private readonly Dictionary<InteractionCard, UpgradeNodeSO> _cardMap = new(2);
        private InteractionCard _originalCard;
        private InteractionCard _currentSelectedCard;

        
        private void Awake()
        {
            SetInteractable(false);
            
            if (cancelButton)
            {
                cancelButton.onClick.AddListener(HandleCloseClicked);
            }

            if (confirmButton)
            {
                confirmButton.onClick.AddListener(HandleConfirmClicked);
            }
        }

        private void OnDestroy()
        {
            if (cancelButton)
            {
                cancelButton.onClick.RemoveListener(HandleCloseClicked);
            }

            if (confirmButton)
            {
                confirmButton.onClick.RemoveListener(HandleConfirmClicked);
            }
        }

        public void Configure(
            InteractionCard cardPrefab,
            CanvasGroup canvasGroup,
            Button confirmButton,
            Button cancelButton,
            TextMeshProUGUI costText,
            Transform originalAnchor,
            Transform next_1_0Anchor,
            Transform next_2_0Anchor,
            Transform next_2_1Anchor)
        {
            this.cardPrefab = cardPrefab;
            this.canvasGroup = canvasGroup;
            this.confirmButton = confirmButton;
            this.cancelButton = cancelButton;
            this.costText = costText;
            this.originalAnchor = originalAnchor;
            this.next_1_0Anchor = next_1_0Anchor;
            this.next_2_0Anchor = next_2_0Anchor;
            this.next_2_1Anchor = next_2_1Anchor;

            if (this.cancelButton)
            {
                this.cancelButton.onClick.RemoveListener(HandleCloseClicked);
                this.cancelButton.onClick.AddListener(HandleCloseClicked);
            }

            if (this.confirmButton)
            {
                this.confirmButton.onClick.RemoveListener(HandleConfirmClicked);
                this.confirmButton.onClick.AddListener(HandleConfirmClicked);
            }

            SetInteractable(false);
        }

        public void Create(CardVisualInput originalVisualInput, 
            CardUpgradePresenter.UpgradeData candidateData)
        {
            ClearCards();
            CreateOriginalCard(originalVisualInput, originalAnchor.position);
            var candidate = CreateCandidateCard(candidateData, next_1_0Anchor.position);
            HandleCardSelected(candidate);
        }

        public void Create(CardVisualInput originalVisualInput,
            CardUpgradePresenter.UpgradeData candidate1Data,
            CardUpgradePresenter.UpgradeData candidate2Data)
        {
            ClearCards();
            CreateOriginalCard(originalVisualInput, originalAnchor.position);
            var candidate1 = CreateCandidateCard(candidate1Data, next_2_0Anchor.position);
            var candidate2 = CreateCandidateCard(candidate2Data, next_2_1Anchor.position);

            candidate1.PointerUp += HandleCardSelected;
            candidate2.PointerUp += HandleCardSelected;
        }

        /// <summary>
        /// 일단은 Presenter에서 돈이 부족하지 않은지만 체크해서 주입함.
        /// </summary>
        public void ValidCanUpgrade(bool canUpgrade)
        {
            if (confirmButton)
            {
                confirmButton.interactable = canUpgrade;
            }
        }

        public async UniTask PlayOpenAnimationAsync()
        {
            await FadeIn();
        }

        public async UniTask PlayCloseAnimationAsync()
        {
            await FadeOut();
        }

        private async UniTask FadeIn()
        {
            foreach (var card in _cardMap.Keys)
            {
                card.VisualController.Fade(0f, true);
                _ = card.VisualController.DoFade(1f, fadeDuration, Ease.Unset, true);
            }
            if (_originalCard)
            {
                _originalCard.VisualController.Fade(0f, true);
                _ = _originalCard.VisualController.DoFade(1f, fadeDuration, Ease.Unset, true);
            }


            canvasGroup.alpha = 0f;
            await canvasGroup.DOFade(1f, fadeDuration);

            SetInteractable(true);
        }

        private async UniTask FadeOut()
        {
            foreach (var card in _cardMap.Keys)
            {
                card.VisualController.Fade(1f, true);
                _ = card.VisualController.DoFade(0f, fadeDuration, Ease.Unset, true);
            }
            if (_originalCard)
            {
                _originalCard.VisualController.Fade(1f, true);
                _ = _originalCard.VisualController.DoFade(0f, fadeDuration, Ease.Unset, true);
            }
            
            canvasGroup.alpha = 1f;
            await canvasGroup.DOFade(0f, fadeDuration);
            
            SetInteractable(false);

            ClearCards();
        }

        private void HandleCardSelected(InteractionCard selected)
        {
            if (_currentSelectedCard == selected) return;
            
            _currentSelectedCard = selected;
            
            var node = _cardMap[selected];
            if (costText)
            {
                costText.text = node.MarketCost + "G";
            }
            
            SelectedNodeChanged?.Invoke(node);
        }

        private void HandleConfirmClicked()
        {
            if (!_currentSelectedCard) return;
            
            ConfirmClicked?.Invoke(_cardMap[_currentSelectedCard]);
        }

        private void HandleCloseClicked()
        {
            CloseClicked?.Invoke();
        }
        
        private InteractionCard CreateOriginalCard(CardVisualInput originalVisualInput, Vector2 position)
        {
            var card = Instantiate(cardPrefab, transform);
            card.Initialize(originalVisualInput, false, LayerMask.NameToLayer("ShopCard"));
            
            card.VisualController.SetSortingOrderLast(CardLayer.PopUp);
            card.FollowTargetPosition = false;
            card.transform.position = position;

            _originalCard = card;
            return card;
        }

        private InteractionCard CreateCandidateCard(CardUpgradePresenter.UpgradeData data, Vector2 position)
        {
            var card = Instantiate(cardPrefab, transform);
            card.Initialize(data.VisualInput, false, LayerMask.NameToLayer("ShopCard"));
            
            card.VisualController.SetSortingOrderLast(CardLayer.PopUp);
            card.FollowTargetPosition = false;
            card.transform.position = position;
            
            _cardMap.Add(card, data.UpgradeNode);

            return card;
        }

        private void ClearCards()
        {
            foreach (var card in _cardMap.Keys)
            {
                if (!card) continue;

                card.PointerUp -= HandleCardSelected;
                Destroy(card.gameObject);
            }

            _cardMap.Clear();

            if (_originalCard)
            {
                Destroy(_originalCard.gameObject);
            }

            _originalCard = null;
            _currentSelectedCard = null;
        }

        private void SetInteractable(bool value)
        {
            if (!canvasGroup) return;

            canvasGroup.blocksRaycasts = value;
            canvasGroup.interactable = value;
        }
    }
}
