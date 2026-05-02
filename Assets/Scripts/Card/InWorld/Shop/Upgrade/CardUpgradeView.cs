using Cardevil.Card.Common;
using Cardevil.Card.Common.Core.Upgrade;
using Cardevil.Card.Common.Visual;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Cardevil.Card.InWorld.Shop.Upgrade
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
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
            
            cancelButton.onClick.AddListener(() => CloseClicked?.Invoke());
            confirmButton.onClick.AddListener(HandleConfirmClicked);
        }

        public void Create(CardVisualInput originalVisualInput, 
            CardUpgradePresenter.UpgradeData candidateData)
        {
            CreateOriginalCard(originalVisualInput, originalAnchor.position);
            var candidate = CreateCandidateCard(candidateData, next_1_0Anchor.position);
            HandleCardSelected(candidate);
        }

        public void Create(CardVisualInput originalVisualInput,
            CardUpgradePresenter.UpgradeData candidate1Data,
            CardUpgradePresenter.UpgradeData candidate2Data)
        {
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
            confirmButton.interactable = canUpgrade;
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
                card.VisualController.DoFade(1f, fadeDuration, Ease.Unset, true);
            }
            _originalCard.VisualController.Fade(0f, true);
            _originalCard.VisualController.DoFade(1f, fadeDuration, Ease.Unset, true);


            canvasGroup.alpha = 0f;
            await canvasGroup.DOFade(1f, fadeDuration);

            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
        }

        private async UniTask FadeOut()
        {
            foreach (var card in _cardMap.Keys)
            {
                card.VisualController.Fade(1f, true);
                card.VisualController.DoFade(0f, fadeDuration, Ease.Unset, true);
            }
            _originalCard.VisualController.Fade(1f, true);
            _originalCard.VisualController.DoFade(0f, fadeDuration, Ease.Unset, true);
            
            canvasGroup.alpha = 1f;
            await canvasGroup.DOFade(0f, fadeDuration);
            
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;

            foreach (var card in _cardMap.Keys)
            {
                Destroy(card.gameObject);
            }
            _cardMap.Clear();
            Destroy(_originalCard.gameObject);
            // _originalCard = null;
        }

        private void HandleCardSelected(InteractionCard selected)
        {
            if (_currentSelectedCard == selected) return;
            
            _currentSelectedCard = selected;
            
            var node = _cardMap[selected];
            costText.text = node.MarketCost + "G";
            
            SelectedNodeChanged?.Invoke(node);
        }

        private void HandleConfirmClicked()
        {
            if (!_currentSelectedCard) return;
            
            ConfirmClicked?.Invoke(_cardMap[_currentSelectedCard]);
        }
        
        private InteractionCard CreateOriginalCard(CardVisualInput originalVisualInput, Vector2 position)
        {
            var card = Instantiate(cardPrefab, transform);
            card.Initialize(originalVisualInput, false, LayerMask.NameToLayer("ShopCard"));
            
            card.VisualController.SetSortingOrderLast();
            card.FollowTargetPosition = false;
            card.transform.position = position;

            _originalCard = card;
            return card;
        }

        private InteractionCard CreateCandidateCard(CardUpgradePresenter.UpgradeData data, Vector2 position)
        {
            var card = Instantiate(cardPrefab, transform);
            card.Initialize(data.VisualInput, false, LayerMask.NameToLayer("ShopCard"));
            
            card.VisualController.SetSortingOrderLast();
            card.FollowTargetPosition = false;
            card.transform.position = position;
            
            _cardMap.Add(card, data.UpgradeNode);

            return card;
        }
    }
}