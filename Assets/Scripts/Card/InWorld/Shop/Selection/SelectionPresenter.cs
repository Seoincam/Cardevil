using Cardevil.Card.Common.Core;
using Cardevil.Card.Common.Core.Upgrade;
using Cardevil.Card.Common.Visual;
using Cardevil.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Cardevil.Card.InWorld.Shop
{
    public class SelectionPresenter
    {
        /// <summary>
        /// 카드가 선택됐을 때 발행되는 이벤트. 선택된 카드의 ID를 인자로 함.
        /// </summary>
        public event Action<int> CardSelected;
        
        
        private readonly CardRepository _repository;
        private readonly ShopCardSelectionView _view;

        private IReadOnlyList<SelectionData> _dataList;

        public struct SelectionData
        {
            public int Id;
            public CardVisualInput VisualInput;
        }

        
        public SelectionPresenter(CardRepository repository, ShopCardSelectionView view, int count)
        {
            _repository = repository;
            _view = view;

            Draw(count);
            _view.CardSelected += id => CardSelected?.Invoke(id);
        }

        
        public void Open(Vector2 centerAnchor) => _view.Open(_dataList, centerAnchor);
        public void Open(Vector2 centerAnchor, int colCount, float colSpacing, float rowSpacing) =>
            _view.Open(_dataList, centerAnchor, colCount, colSpacing, rowSpacing);
        
        /// <summary>
        /// 특정 카드가 업그레이드되었을 때 호출될 콜백.
        /// </summary>
        public void HandleCardUpgraded(int specId)
        {
            var newVisualInput = CardVisualInput.From(_repository.GetNewState(specId));
            _view.UpdateCardVisual(specId, newVisualInput);
        }

        
        private void Draw(int count)
        {
            var list = new List<SelectionData>(count);
            var drawResultIds = ShopCardProvider.DrawShopCards(_repository.Cards, count);
            foreach (var id in drawResultIds)
            {
                var visualInput = CardVisualInput.From(_repository.GetNewState(id));
                list.Add(new SelectionData
                {
                    Id = id, 
                    VisualInput = visualInput
                });
            }
            _dataList = list;
        }

        
        #region 카드 뽑기
        private static class ShopCardProvider
        {
            private enum CardCategory
            {
                Red,
                Green,
                Blue,
                Black,
                Direction
            }
            
            /// <summary>
            /// 상점용 카드들을 가중치와 규칙에 따라 추첨해 반환.
            /// </summary>
            public static List<int> DrawShopCards(IReadOnlyCollection<CardSpec> allSpecs, int drawCount)
            {
                var resultIds = new List<int>();
    
                // 1. 카테고리별로 뽑기 가능한 카드 분류
                var availableSpecs = new Dictionary<CardCategory, List<CardSpec>>();
                var categoryWeights = new Dictionary<CardCategory, int>();
                var drawCounts = new Dictionary<CardCategory, int>();
    
                foreach (CardCategory category in Enum.GetValues(typeof(CardCategory)))
                {
                    availableSpecs[category] = new List<CardSpec>();
                    categoryWeights[category] = 0;
                    drawCounts[category] = 0;
                }
    
                // 전체 카드 순회해 분류 & 카테고리 별 가중치 계산
                foreach (var spec in allSpecs)
                {
                    var category = GetCategory(spec);
                    
                    // 카테고리 가중치 합산
                    categoryWeights[category] = GetCategoryWeight(spec);
    
                    // 뽑기 가능한 (최종 강화 아닌) 카드만 폴에 추가
                    if (GetIndividualWeight(spec) > 0)
                    {
                        availableSpecs[category].Add(spec);
                    }
                }
                
                // 2. 각 종류별 최소 1장씩 확정 추첨
                foreach (CardCategory category in Enum.GetValues(typeof(CardCategory)))
                {
                    if (availableSpecs[category].Count > 0 && resultIds.Count < drawCount)
                    {
                        var pickedSpec = PickCardFromCategory(availableSpecs[category]);
                        
                        resultIds.Add(pickedSpec.ID);
                        availableSpecs[category].Remove(pickedSpec);
                        drawCounts[category]++;
                    }
                }
                
                // 3. 남은 카드 추첨
                while (resultIds.Count < drawCount)
                {
                    // 카테고리 개수 초과 안 하고, 뽑을 수 있는 카드가 남아있는 카테고리 필터링
                    var validCategories = categoryWeights.Keys.Where(category =>
                        drawCounts[category] < GetMaxLimitCount(category) &&
                        availableSpecs[category].Count > 0
                    ).ToList();
                    
                    if (validCategories.Count == 0) break;
    
                    // 카테고리 가중치에 따라 재추첨
                    int totalCategoryWeight = validCategories.Sum(category => categoryWeights[category]);
                    int categoryRoll = RandomUtil.GetRandomInt(0, totalCategoryWeight);
    
                    CardCategory selectedCategory = validCategories[0];
                    int currentWeight = 0;
    
                    foreach (var category in validCategories)
                    {
                        currentWeight += categoryWeights[category];
                        if (currentWeight > categoryRoll)
                        {
                            selectedCategory = category;
                            break;
                        }
                    }
                    
                    // 선택된 카테고리 내에서 개별 카드 가중치 추첨
                    var pickedSpec = PickCardFromCategory(availableSpecs[selectedCategory]);
                    resultIds.Add(pickedSpec.ID);
                    availableSpecs[selectedCategory].Remove(pickedSpec);
                    drawCounts[selectedCategory]++;
                }
    
                return resultIds;
            }
    
            /// <summary>
            /// 카테고리 내에서 개별 카드의 가중치를 기반으로 하나 뽑아 반환.
            /// </summary>
            private static CardSpec PickCardFromCategory(IReadOnlyCollection<CardSpec> specs)
            {
                int totalWeight = specs.Sum(GetIndividualWeight);
                int roll = RandomUtil.GetRandomInt(0, totalWeight); // TODO: 랜덤 유틸 제대로된 카테고리 지정
                int currentWeight = 0;
    
                foreach (var spec in specs)
                {
                    currentWeight += GetIndividualWeight(spec);
                    if (currentWeight > roll)
                    {
                        return spec;
                    }
                }
    
                return specs.Last(); // 혹시 몰라서.
            }
    
            /// <summary>
            /// 카드의 카테고리를 반환.
            /// </summary>
            /// <remarks> 색이 존재하지 않을 경우 빨강으로 폴백함. </remarks>
            private static CardCategory GetCategory(CardSpec spec)
            {
                if (spec.IsMove) return CardCategory.Direction;

                var color = spec.State.ColorList.DefaultValue.HasValue
                    ? spec.State.ColorList.DefaultValue.Value
                    : CardColor.None;
                
                return color switch
                {
                    CardColor.Red => CardCategory.Red,
                    CardColor.Green => CardCategory.Green,
                    CardColor.Blue => CardCategory.Blue,
                    CardColor.Black => CardCategory.Black,
                    
                    _ => CardCategory.Red // 폴백
                };
            }
    
            /// <summary>
            /// 종류별로 나올 수 있는 카드의 수를 반환.
            /// </summary>
            private static int GetMaxLimitCount(CardCategory category)
            {
                return category == CardCategory.Direction ? 3 : 4;
            }
    
            /// <summary>
            /// 카테고리 추첨 시 사용되는 카드의 가중치를 반환.
            /// </summary>
            private static int GetCategoryWeight(CardSpec spec)
            {
                var (upgradePath, upgradeStage) = GetUpgradeStatus(spec);
    
                if (spec.IsAttack)
                {
                    if (upgradePath == UpgradePath.MultiColor)
                    {
                        return upgradeStage switch { 0 => 1, 1 => 2, 2 => 3, 3 => 6, _ => 0 };
                    }
                    else
                    {
                        return upgradeStage switch { 0 => 1, 1 => 2, 2 => 3, 3 => 6, _ => 0 };
                    }
                }
                // IsMove
                else
                {
                    return upgradeStage switch { 0 => 1, 1 => 2, 2 => 4, _ => 0 };
                }
            }
    
            /// <summary>
            /// 개별 카드 추첨 시 강화 단계에 따라 계산되는 가중치를 반환. 최종 단계 강화일 경우 0을 반환함.
            /// </summary>
            private static int GetIndividualWeight(CardSpec spec)
            {
                var (upgradePath, upgradeStage) = GetUpgradeStatus(spec);
    
                if (spec.IsAttack)
                {
                    if (upgradePath == UpgradePath.MultiColor)
                    {
                        return upgradeStage switch { 0 => 1, 1 => 3, 2 => 4, 3 => 0, _ => 0 };
                    }
                    else
                    {
                        return upgradeStage switch { 0 => 1, 1 => 2, 2 => 3, 3 => 0, _ => 0 };
                    }
                }
                // IsMove
                else
                {
                    return upgradeStage switch { 0 => 1, 1 => 2, 2 => 0, _ => 0 };
                }
            }
    
            private static (UpgradePath path, int stage) GetUpgradeStatus(CardSpec spec)
            {
                if (spec.UpgradeNode == null)
                    return (UpgradePath.None, 0);
                
                return (spec.UpgradeNode.Path, spec.UpgradeNode.Stage);
            }
        }
        #endregion
    }
}