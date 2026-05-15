using Cardevil.Card.Common.Core;
using Cardevil.Card.Common.Core.Upgrade;
using Cardevil.Card.InWorld.Shop.Upgrade;
using Cardevil.Core.Bootstrap;
using System.Collections.Generic;
using UnityEngine;

namespace Cardevil.Card.InWorld.Shop
{
    public class ShopCardManager : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField, Tooltip("추첨할 카드의 총 개수")] private int drawCardCount = 10;
        
        [Header("Scene References")]
        [SerializeField] private ShopCardSelectionView shopCardSelectionView;
        [SerializeField] private CardUpgradeView upgradeView;
        
        [Header("References")]
        [SerializeField] private UpgradeNodeDatabaseSO upgradeDatabase;
        
        private SelectionPresenter _selectionPresenter;
        private CardUpgradePresenter _upgradePresenter;

        private CardRepository Repository => CardevilCore.Game.CardRepository;


        private void Awake()
        {
            _selectionPresenter = new SelectionPresenter(Repository, shopCardSelectionView, drawCardCount);
            _upgradePresenter = new CardUpgradePresenter(Repository, upgradeDatabase, upgradeView);

            _selectionPresenter.CardSelected += _upgradePresenter.HandleCardSelected;
            _upgradePresenter.CardUpgraded += _selectionPresenter.HandleCardUpgraded;
        }

        
        /// <summary>
        /// 지정된 개수만큼 상점 카드를 추첨하고, 그리드 형식으로 선택 화면을 생성.
        /// </summary>
        /// <param name="centerAnchor">중앙 기준점이 될 Transform.</param>
        public void OpenSelection(Transform centerAnchor) => OpenSelection(centerAnchor.position);
        
        /// <summary>
        /// 지정된 개수만큼 상점 카드를 추첨하고, 그리드 형식으로 선택 화면을 생성.
        /// </summary>
        /// <param name="centerAnchor">전체 카드 뭉치가 배치될 중앙 기준 좌표.</param>
        public void OpenSelection(Vector2 centerAnchor) => _selectionPresenter.Open(centerAnchor);

        /// <summary>
        /// 지정된 개수만큼 상점 카드를 추첨하고, 그리드 형식으로 선택 화면을 생성.
        /// </summary>
        /// <param name="centerAnchor">중앙 기준점이 될 Transform.</param>
        /// <param name="colCount">가로로 배치될 카드 개수(열 개수).</param>
        /// <param name="colSpacing">카드 사이의 가로 간격.</param>
        /// <param name="rowSpacing">줄 사이의 세로 간격.</param>
        public void OpenSelection(Transform centerAnchor, int colCount, float colSpacing, float rowSpacing) =>
            OpenSelection(centerAnchor.position, colCount, colSpacing, rowSpacing);
        
        /// <summary>
        /// 지정된 개수만큼 상점 카드를 추첨하고, 그리드 형식으로 선택 화면을 생성.
        /// </summary>
        /// <param name="centerAnchor">전체 카드 뭉치가 배치될 중앙 기준 좌표.</param>
        /// <param name="colCount">가로로 배치될 카드 개수(열 개수).</param>
        /// <param name="colSpacing">카드 사이의 가로 간격(Column Spacing).</param>
        /// <param name="rowSpacing">줄 사이의 세로 간격(Row Spacing).</param>
        public void OpenSelection(Vector2 centerAnchor, int colCount, float colSpacing, float rowSpacing) =>
            _selectionPresenter.Open(centerAnchor, colCount, colSpacing, rowSpacing);
        

        [ContextMenu("Create Example")]
        private void CreateExample()
        {
            OpenSelection(Vector2.zero);
        }
    }
}