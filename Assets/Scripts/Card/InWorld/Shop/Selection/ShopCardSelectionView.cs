using Cardevil.Card.Common;
using Cardevil.Card.Common.Visual;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Cardevil.Card.InWorld.Shop
{
    public class ShopCardSelectionView : MonoBehaviour
    {
        /// <summary>
        /// 카드가 선택됐을 때 발행되는 이벤트. 선택된 카드의 ID를 인자로 함.
        /// </summary>
        public event Action<int> CardSelected;
        
        
        [Header("Prefabs")]
        [SerializeField] private InteractionCard cardPrefab;
        
        [Header("Scene References")]
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Settings")] 
        [SerializeField] private float fadeDuration = 1f;
        
        [Header("Default Settings")]
        [SerializeField] private int defaultColCount = 5;
        [SerializeField] private float defaultColSpacing = 3f;
        [SerializeField] private float defaultRowSpacing = 2.75f;
        
        private readonly Dictionary<InteractionCard, int> _createdCardMap = new();
        private readonly Dictionary<int, InteractionCard> _reverseCreatedCardMap = new();

        
        private void Awake()
        {
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }

        
        public void Open(IReadOnlyList<SelectionPresenter.SelectionData> dataList, Vector2 centerAnchor)
        {
            CreateAndAlignCards(dataList, centerAnchor, defaultColCount, defaultColSpacing, defaultRowSpacing);
            FadeIn().Forget();
        }

        public void Open(IReadOnlyList<SelectionPresenter.SelectionData> dataList,
            Vector2 centerAnchor,
            int colCount,
            float colSpacing,
            float rowSpacing)
        {
            CreateAndAlignCards(dataList, centerAnchor, colCount, colSpacing, rowSpacing);
            FadeIn().Forget();
        }

        public void UpdateCardVisual(int id, CardVisualInput newVisualInput)
        {
            var targetCard = _reverseCreatedCardMap[id];
            targetCard.VisualController.SetLayout(newVisualInput);
        }

        public void Close()
        {
            
        }
        

        
        [ContextMenu("Clear Cards")]
        public void Clear()
        {
            foreach (var card in _createdCardMap.Keys)
            {
                Destroy(card.gameObject);
            }
            _createdCardMap.Clear();
        }

        public async UniTaskVoid FadeIn()
        {
            foreach (var card in _createdCardMap.Keys)
            {
                card.VisualController.Fade(0f, true);
                card.VisualController.DoFade(1f, fadeDuration, Ease.Unset, true);
            }

            canvasGroup.alpha = 0f;
            await canvasGroup.DOFade(1f, fadeDuration);
            
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
        }

        public async UniTaskVoid FadeOut()
        {
            foreach (var card in _createdCardMap.Keys)
            {
                card.VisualController.Fade(1f, true);
                card.VisualController.DoFade(0f, fadeDuration, Ease.Unset, true);
            }
            
            canvasGroup.alpha = 1f;
            await canvasGroup.DOFade(0f, fadeDuration);
            
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }

        
        private void CreateAndAlignCards(IReadOnlyList<SelectionPresenter.SelectionData> dataList, 
            Vector2 centerAnchor,
            int colCount,
            float colSpacing,
            float rowSpacing)
        {
            int totalCount = dataList.Count;
            if (totalCount == 0) return;

            // 전체 Row 개수 계산
            int rowCount = Mathf.CeilToInt((float)totalCount / colCount);

            // 실제 배 배치될 영역의 가로/세로 전체 크기 계산
            // (칸 개수 - 1) * 간격
            int actualCols = Mathf.Min(totalCount, colCount);
            float totalWidth = (actualCols - 1) * colSpacing;
            float totalHeight = (rowCount - 1) * rowSpacing;

            // 중앙 정렬을 위한 시작점(좌측 상단) 오프셋 계산
            // centerAnchor에서 전체 크기의 절반만큼 왼쪽, 위쪽으로 이동
            Vector2 startOffset = new(-totalWidth / 2f, totalHeight / 2f);

            int index = 0;
            foreach (var data in dataList)
            {
                // 현재 인덱스에 따른 Row, Col 도출
                int r = index / colCount;
                int c = index % colCount;

                // 로컬 좌표 계산 (Y는 아래로 갈수록 작아지므로 -rowSpacing)
                float x = c * colSpacing;
                float y = -(r * rowSpacing);

                Vector2 finalPosition = centerAnchor + startOffset + new Vector2(x, y);

                CreateCard(data, finalPosition);
                index++;
            }
        }

        private InteractionCard CreateCard(SelectionPresenter.SelectionData data, Vector2 position)
        {
            var card = Instantiate(cardPrefab, transform);
            card.Initialize(data.VisualInput, false, LayerMask.NameToLayer("ShopCard"));
            
            card.VisualController.SetSortingOrder(10);
            card.FollowTargetPosition = false;
            card.transform.position = position;
            
            _createdCardMap[card] = data.Id;
            _reverseCreatedCardMap[data.Id] = card;
            
            card.PointerUp += HandleCardSelected;
            return card;
        }

        private void HandleCardSelected(InteractionCard selected)
        {
            CardSelected?.Invoke(_createdCardMap[selected]);
        }
    }
}