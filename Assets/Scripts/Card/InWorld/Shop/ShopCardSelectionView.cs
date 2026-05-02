using Cardevil.Card.Common;
using Cardevil.Card.Common.Visual;
using Cardevil.Core.Bootstrap;
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
        [SerializeField] private Camera cardCamera;

        private readonly List<InteractionCard> _createdCards = new();
        
        public void CreateSelectionView(
            IReadOnlyCollection<int> cardIds,
            Vector2 centerAnchor,
            int colCount,
            float cardSpacing,
            float rowSpacing)
        {
            int totalCount = cardIds.Count;
            if (totalCount == 0) return;

            // 전체 행(Row) 개수 계산
            int rowCount = Mathf.CeilToInt((float)totalCount / colCount);

            // 실제 배 배치될 영역의 가로/세로 전체 크기 계산
            // (칸 개수 - 1) * 간격
            int actualCols = Mathf.Min(totalCount, colCount);
            float totalWidth = (actualCols - 1) * cardSpacing;
            float totalHeight = (rowCount - 1) * rowSpacing;

            // 중앙 정렬을 위한 시작점(좌측 상단) 오프셋 계산
            // centerAnchor에서 전체 크기의 절반만큼 왼쪽(-), 위쪽(+)으로 이동
            Vector2 startOffset = new Vector2(-totalWidth / 2f, totalHeight / 2f);

            int index = 0;
            foreach (var id in cardIds)
            {
                // 현재 인덱스에 따른 행(Row)과 열(Col) 도출
                int r = index / colCount;
                int c = index % colCount;

                // 로컬 좌표 계산 (Y는 아래로 갈수록 작아지므로 -rowSpacing)
                float x = c * cardSpacing;
                float y = -(r * rowSpacing);

                Vector2 finalPosition = centerAnchor + startOffset + new Vector2(x, y);

                var card = CreateCard(id, finalPosition);
                _createdCards.Add(card);
                index++;
            }
        }

        [ContextMenu("Clear Cards")]
        public void Clear()
        {
            for (int i = _createdCards.Count - 1; i >= 0; i--)
            {
                var card = _createdCards[i];
                Destroy(card.gameObject);
            }
            
            _createdCards.Clear();
        }

        private InteractionCard CreateCard(int id, Vector2 position)
        {
            var state = CardevilCore.Game.CardRepository.GetState(id);
            var visualInput = CardVisualInput.From(state);
            
            var card = Instantiate(cardPrefab, transform);
            card.Initialize(visualInput, cardCamera);
            card.VisualController.SetSortingOrderLast();
            card.FollowTargetPosition = false;
            card.transform.position = position;

            int currentId = id;
            card.PointerUp += _ => CardSelected?.Invoke(currentId);

            return card;
        }
    }
}