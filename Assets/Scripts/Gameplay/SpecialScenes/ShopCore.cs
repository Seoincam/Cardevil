using Cardevil.Card.Common.Core;
using Cardevil.Card.Common.Core.Upgrade;
using Cardevil.Card.Common.Visual;
using Cardevil.Core;
using Cardevil.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Cardevil.Gameplay.SpecialScenes
{
    [System.Serializable]
    public class ShopCore : SpecialSceneCore
    {
        private const int ConsumableEntryCount = 5;
        private const int ReinforceEntryCount = 10;
        private const string RedCardIconPath = "Arts/Card/Card_Base/Card_Red/Card_Red_Star";
        private const string GreenCardIconPath = "Arts/Card/Card_Base/Card_Green/Card_Green_Star";
        private const string BlueCardIconPath = "Arts/Card/Card_Base/Card_Blue/Card_Blue_Star";
        private const string BlackCardIconPath = "Arts/Card/Card_Base/Card_Black/Card_Black_Star";
        private const string RelicChestIconPath = "Arts/InStage/ClearReward/Relic Chest/Relic_Chest_Devil_Hand";

        public override string TestTitle => "Shop";
        public override string TestDescription => $"Floor {Context.floor} shop entrance.\nThis is the initial shell scene. Closing it clears the shop node and returns to the map.";
        public override Color TestAccentColor => new(0.72f, 0.54f, 0.18f, 0.96f);
        
        
        [Header("Run-time Data")]
        [SerializeField] private List<ShopEntryData> consumableEntries = new(ConsumableEntryCount);
        [SerializeField] private List<ShopEntryData> reinforceEntries = new(ReinforceEntryCount);
        
        private UpgradeNodeDatabaseSO _upgradeDatabase;
        
        public IReadOnlyList<ShopEntryData> ConsumableEntries => consumableEntries;
        public IReadOnlyList<ShopEntryData> ReinforceEntries => reinforceEntries;
        public int ReinforceDrawCount => ReinforceEntryCount;

        public void Configure(UpgradeNodeDatabaseSO upgradeDatabase)
        {
            _upgradeDatabase = upgradeDatabase;
        }
        
        public override void Initialize(GameFlowManager.SpecialSceneEnterContext context)
        {
            base.Initialize(context);
            GenerateShopEntries();
        }

        public void RefreshCardReinforceEntry(int cardSpecId)
        {
            int index = reinforceEntries.FindIndex(entry =>
                entry.Kind == ShopEntryKind.CardReinforce && entry.CardSpecId == cardSpecId);
            if (index < 0)
            {
                return;
            }

            var refreshedEntry = CreateCardReinforceEntry(Cardevil.Core.Bootstrap.CardevilCore.Game.CardRepository.GetSpec(cardSpecId));
            if (!refreshedEntry.IsAvailable)
            {
                reinforceEntries[index] = new ShopEntryData
                {
                    Kind = ShopEntryKind.CardReinforce,
                    CardSpecId = cardSpecId,
                    IsAvailable = false
                };
                return;
            }

            refreshedEntry.Discount = reinforceEntries[index].Discount;
            reinforceEntries[index] = refreshedEntry;
        }

        public void MarkConsumablePurchased(ShopEntryData purchasedEntry)
        {
            int index = consumableEntries.FindIndex(entry =>
                entry.IsAvailable &&
                entry.Kind == ShopEntryKind.Consumable &&
                entry.ConsumableKind == purchasedEntry.ConsumableKind &&
                entry.TooltipKey == purchasedEntry.TooltipKey);

            if (index < 0)
            {
                return;
            }

            var unavailableEntry = consumableEntries[index];
            unavailableEntry.IsAvailable = false;
            consumableEntries[index] = unavailableEntry;
        }
        
        private void GenerateShopEntries()
        {
            GenerateConsumableEntries();
            GenerateReinforceEntries();
        }

        private void GenerateConsumableEntries()
        {
            consumableEntries.Clear();
            consumableEntries.Add(ShopEntryData.Consumable(
                ShopConsumableKind.RedCard,
                3,
                "tooltip.item.redcard",
                RedCardIconPath,
                new ShopDiscount { FixedGold = 1 }));
            consumableEntries.Add(ShopEntryData.Consumable(
                ShopConsumableKind.BlueCard,
                2,
                "tooltip.item.bluecard",
                BlueCardIconPath));
            consumableEntries.Add(ShopEntryData.Consumable(
                ShopConsumableKind.GreenCard,
                2,
                "tooltip.item.greencard",
                GreenCardIconPath));
            consumableEntries.Add(ShopEntryData.Consumable(
                ShopConsumableKind.BlackCard,
                4,
                "tooltip.item.blackcard",
                BlackCardIconPath,
                new ShopDiscount { Percent = 0.25f }));
            consumableEntries.Add(ShopEntryData.Consumable(
                ShopConsumableKind.RelicChest,
                5,
                "tooltip.shop.relic-chest",
                RelicChestIconPath));
        }

        private void GenerateReinforceEntries()
        {
            reinforceEntries.Clear();

            if (!_upgradeDatabase)
            {
                return;
            }

            var repository = Cardevil.Core.Bootstrap.CardevilCore.Game.CardRepository;
            var reinforceSpecs = ShopReinforceCardProvider.DrawReinforceableCards(
                repository.Cards,
                _upgradeDatabase,
                ReinforceDrawCount);

            foreach (var spec in reinforceSpecs)
            {
                var entry = CreateCardReinforceEntry(spec);
                if (entry.IsAvailable)
                {
                    entry.Discount = CreateReinforceDiscount(reinforceEntries.Count, entry.BaseGoldCost);
                    reinforceEntries.Add(entry);
                }
            }
        }

        private ShopEntryData CreateCardReinforceEntry(CardSpec spec)
        {
            if (spec == null || !_upgradeDatabase)
            {
                return new ShopEntryData { Kind = ShopEntryKind.CardReinforce, IsAvailable = false };
            }

            var availableNodes = _upgradeDatabase.GetNextAvailableNodes(spec);
            if (availableNodes.Count == 0)
            {
                return new ShopEntryData
                {
                    Kind = ShopEntryKind.CardReinforce,
                    CardSpecId = spec.ID,
                    IsAvailable = false
                };
            }

            int baseCost = availableNodes.Min(node => node.MarketCost);
            return ShopEntryData.CardReinforce(
                spec.ID,
                CardVisualInput.From(spec.State),
                baseCost,
                "tooltip.shop.card-upgrade");
        }

        private static ShopDiscount CreateReinforceDiscount(int index, int baseCost)
        {
            if (baseCost <= 0)
            {
                return default;
            }

            return index switch
            {
                0 => new ShopDiscount { Percent = 0.25f },
                4 => new ShopDiscount { FixedGold = 1 },
                _ => default
            };
        }
    }

    public static class ShopReinforceCardProvider
    {
        private enum CardCategory
        {
            Red,
            Green,
            Blue,
            Black,
            Direction
        }

        public static List<CardSpec> DrawReinforceableCards(
            IReadOnlyCollection<CardSpec> allSpecs,
            UpgradeNodeDatabaseSO upgradeDatabase,
            int drawCount)
        {
            var result = new List<CardSpec>();
            if (allSpecs == null || !upgradeDatabase || drawCount <= 0)
            {
                return result;
            }

            var availableSpecs = new Dictionary<CardCategory, List<CardSpec>>();
            var categoryWeights = new Dictionary<CardCategory, int>();
            var drawCounts = new Dictionary<CardCategory, int>();

            foreach (CardCategory category in Enum.GetValues(typeof(CardCategory)))
            {
                availableSpecs[category] = new List<CardSpec>();
                categoryWeights[category] = 0;
                drawCounts[category] = 0;
            }

            foreach (var spec in allSpecs)
            {
                if (spec == null || upgradeDatabase.GetNextAvailableNodes(spec).Count == 0)
                {
                    continue;
                }

                var category = GetCategory(spec);
                categoryWeights[category] = Math.Max(categoryWeights[category], GetCategoryWeight(spec));

                if (GetIndividualWeight(spec) > 0)
                {
                    availableSpecs[category].Add(spec);
                }
            }

            foreach (CardCategory category in Enum.GetValues(typeof(CardCategory)))
            {
                if (availableSpecs[category].Count <= 0 || result.Count >= drawCount)
                {
                    continue;
                }

                var pickedSpec = PickCardFromCategory(availableSpecs[category]);
                result.Add(pickedSpec);
                availableSpecs[category].Remove(pickedSpec);
                drawCounts[category]++;
            }

            while (result.Count < drawCount)
            {
                var validCategories = categoryWeights.Keys.Where(category =>
                    drawCounts[category] < GetMaxLimitCount(category) &&
                    availableSpecs[category].Count > 0 &&
                    categoryWeights[category] > 0).ToList();

                if (validCategories.Count == 0)
                {
                    break;
                }

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

                var pickedSpec = PickCardFromCategory(availableSpecs[selectedCategory]);
                result.Add(pickedSpec);
                availableSpecs[selectedCategory].Remove(pickedSpec);
                drawCounts[selectedCategory]++;
            }

            return result;
        }

        private static CardSpec PickCardFromCategory(IReadOnlyCollection<CardSpec> specs)
        {
            int totalWeight = specs.Sum(GetIndividualWeight);
            int roll = RandomUtil.GetRandomInt(0, totalWeight);
            int currentWeight = 0;

            foreach (var spec in specs)
            {
                currentWeight += GetIndividualWeight(spec);
                if (currentWeight > roll)
                {
                    return spec;
                }
            }

            return specs.Last();
        }

        private static CardCategory GetCategory(CardSpec spec)
        {
            if (spec.IsMove)
            {
                return CardCategory.Direction;
            }

            var color = spec.State.ColorList.DefaultValue.HasValue
                ? spec.State.ColorList.DefaultValue.Value
                : CardColor.None;

            return color switch
            {
                CardColor.Red => CardCategory.Red,
                CardColor.Green => CardCategory.Green,
                CardColor.Blue => CardCategory.Blue,
                CardColor.Black => CardCategory.Black,
                _ => CardCategory.Red
            };
        }

        private static int GetMaxLimitCount(CardCategory category)
        {
            return category == CardCategory.Direction ? 3 : 4;
        }

        private static int GetCategoryWeight(CardSpec spec)
        {
            var (_, upgradeStage) = GetUpgradeStatus(spec);

            if (spec.IsAttack)
            {
                return upgradeStage switch { 0 => 1, 1 => 2, 2 => 3, 3 => 6, _ => 0 };
            }

            return upgradeStage switch { 0 => 1, 1 => 2, 2 => 4, _ => 0 };
        }

        private static int GetIndividualWeight(CardSpec spec)
        {
            var (upgradePath, upgradeStage) = GetUpgradeStatus(spec);

            if (spec.IsAttack)
            {
                return upgradePath == UpgradePath.MultiColor
                    ? upgradeStage switch { 0 => 1, 1 => 3, 2 => 4, 3 => 0, _ => 0 }
                    : upgradeStage switch { 0 => 1, 1 => 2, 2 => 3, 3 => 0, _ => 0 };
            }

            return upgradeStage switch { 0 => 1, 1 => 2, 2 => 0, _ => 0 };
        }

        private static (UpgradePath path, int stage) GetUpgradeStatus(CardSpec spec)
        {
            if (spec.UpgradeNode == null)
            {
                return (UpgradePath.None, 0);
            }

            return (spec.UpgradeNode.Path, spec.UpgradeNode.Stage);
        }
    }
}
