using Cardevil.Card.Common.Core;
using Cardevil.Card.Common.Core.Upgrade;
using Cardevil.Core.Bootstrap;
using System.Collections.Generic;
using UnityEngine;

namespace Cardevil.Card.InWorld.Shop
{
    public class ShopCardManager : MonoBehaviour
    {
        [Header("Scene References")]
        [SerializeField] private ShopCardSelectionView shopCardSelectionView;
        
        [Header("References")]
        [SerializeField] private UpgradeNodeDatabaseSO upgradeDatabase;
        
        /// <summary>
        /// 지정된 개수만큼 상점 카드를 추첨하고, 그리드 형식으로 선택 화면을 생성.
        /// </summary>
        /// <param name="count">추첨할 카드의 총 개수.</param>
        /// <param name="centerAnchor">전체 카드 뭉치가 배치될 중앙 기준 좌표.</param>
        /// <param name="colCount">가로로 배치될 카드 개수(열 개수).</param>
        /// <param name="colSpacing">카드 사이의 가로 간격(Column Spacing).</param>
        /// <param name="rowSpacing">줄 사이의 세로 간격(Row Spacing).</param>
        public void CreateSelection(int count, Vector2 centerAnchor, int colCount, float colSpacing, float rowSpacing)
        {
            var allSpecs = CardevilCore.Game.CardRepository.Cards;
            var resultIds = ShopCardProvider.DrawShopCards(allSpecs, count);

            shopCardSelectionView.CreateSelectionView(resultIds, centerAnchor, colCount, colSpacing, rowSpacing);
            
            shopCardSelectionView.CardSelected -= HandleCardSelected;
            shopCardSelectionView.CardSelected += HandleCardSelected;
        }

        /// <summary>
        /// 지정된 개수만큼 상점 카드를 추첨하고, 그리드 형식으로 선택 화면을 생성.
        /// </summary>
        /// <param name="count">추첨할 카드의 총 개수.</param>
        /// <param name="centerAnchor">중앙 기준점이 될 Transform.</param>
        /// <param name="colCount">가로로 배치될 카드 개수(열 개수).</param>
        /// <param name="colSpacing">카드 사이의 가로 간격.</param>
        /// <param name="rowSpacing">줄 사이의 세로 간격.</param>
        public void CreateSelection(int count, Transform centerAnchor, int colCount, float colSpacing, float rowSpacing)
        {
            CreateSelection(count, centerAnchor.position, colCount, colSpacing, rowSpacing);
        }

        public IReadOnlyCollection<UpgradeNodeSO> GetAvailableUpgradeNodes(CardSpec spec)
        {
            return upgradeDatabase.GetNextAvailableNodes(spec);
        }
        

        private void HandleCardSelected(int specId)
        {
            Debug.Log($"카드 선택됐음! ID: {specId}");
        }

        [ContextMenu("Create Example")]
        private void CreateExample()
        {
            CreateSelection(10, Vector2.zero, 5, 3f, 2.75f);
        }
    }
}