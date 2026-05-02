using Cardevil.Card.Common;
using Cardevil.Card.Common.Visual;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Cardevil.Card.InWorld.Shop.Upgrade
{
    public class CardUpgradeView : MonoBehaviour
    {
        public event Action<int> UpgradeRequested;
        
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

        private List<InteractionCard> _createdCards = new(3);
        private int _selectedIndex;

        private void Awake()
        {
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
            
            cancelButton.onClick.AddListener(HandleCancelButtonClicked);
        }

        public void Create(CardVisualInput original, CardVisualInput next)
        {
            CreateCard(original, originalAnchor);
            CreateCard(next, next_1_0Anchor);

            _selectedIndex = 0;
            confirmButton.onClick.RemoveAllListeners();
            confirmButton.onClick.AddListener(HandleConfirmButtonClicked);
        }

        public void Create(CardVisualInput original, CardVisualInput next1, CardVisualInput next2)
        {
            CreateCard(original, originalAnchor);
            
            var next1Card = CreateCard(next1, next_2_0Anchor);
            next1Card.PointerUp += _ => _selectedIndex = 0;
            
            var next2Card = CreateCard(next2, next_2_1Anchor);
            next2Card.PointerUp += _ => _selectedIndex = 1;

            _selectedIndex = 0;
            confirmButton.onClick.RemoveAllListeners();
            confirmButton.onClick.AddListener(HandleConfirmButtonClicked);
        }

        public async UniTask PlayOpenAnimationAsync()
        {
            await FadeIn();
        }

        private async UniTask FadeIn()
        {
            foreach (var card in _createdCards)
            {
                card.VisualController.Fade(0f, true);
                card.VisualController.DoFade(1f, fadeDuration, Ease.Unset, true);
            }

            canvasGroup.alpha = 0f;
            await canvasGroup.DOFade(1f, fadeDuration);

            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
        }

        public async UniTask FadeOut()
        {
            foreach (var card in _createdCards)
            {
                card.VisualController.Fade(1f, true);
                card.VisualController.DoFade(0f, fadeDuration, Ease.Unset, true);
            }
            
            canvasGroup.alpha = 1f;
            await canvasGroup.DOFade(0f, fadeDuration);
            
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;

            for (int i = _createdCards.Count - 1; i >= 0; i--)
            {
                Destroy(_createdCards[i].gameObject);
            } 
            _createdCards.Clear();
        }

        private InteractionCard CreateCard(CardVisualInput visualInput, Transform anchor)
        {
            return CreateCard(visualInput, anchor.position);
        }
        
        private InteractionCard CreateCard(CardVisualInput visualInput, Vector2 position)
        {
            var card = Instantiate(cardPrefab, transform);
            card.Initialize(visualInput, false, LayerMask.NameToLayer("ShopCard"));
            
            card.VisualController.SetSortingOrderLast();
            card.FollowTargetPosition = false;
            card.transform.position = position;

            _createdCards.Add(card);
            return card;
        }

        private void HandleCardSelected(int index)
        {
            
        }

        private void HandleConfirmButtonClicked()
        {
            UpgradeRequested?.Invoke(_selectedIndex);
        }

        private void HandleCancelButtonClicked()
        {
            FadeOut().Forget();
        }
    }
}