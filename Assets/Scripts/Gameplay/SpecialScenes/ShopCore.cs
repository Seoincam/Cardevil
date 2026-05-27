using Cardevil.Core;
using Cardevil.Core.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace Cardevil.Gameplay.SpecialScenes
{
    [System.Serializable]
    public class ShopCore : SpecialSceneCore
    {
        public override string TestTitle => "Shop";
        public override string TestDescription => $"Floor {Context.floor} shop entrance.\nThis is the initial shell scene. Closing it clears the shop node and returns to the map.";
        public override Color TestAccentColor => new(0.72f, 0.54f, 0.18f, 0.96f);
        
        
        [Header("Run-time Data")]
        [SerializeField] private List<ShopEntryData> shopEntries = new List<ShopEntryData>();
        
        
        public List<ShopEntryData> ShopEntries => shopEntries;
        public int ReinforceDrawCount => 10;
        
        public override void Initialize(GameFlowManager.SpecialSceneEnterContext context)
        {
            base.Initialize(context);
            GenerateShopEntries();
        }
        
        private void GenerateShopEntries()
        {
            shopEntries.Clear();
            shopEntries.Add(new ShopEntryData
            {
                goldCost = RandomUtil.GetRandomInt(0, 3),
                TooltipKey = "tooltip.item.red"
            });
            shopEntries.Add(new ShopEntryData
            {
                goldCost = RandomUtil.GetRandomInt(0, 3),
                TooltipKey = "tooltip.item.blue"
            });
            shopEntries.Add(new ShopEntryData
            {
                goldCost = RandomUtil.GetRandomInt(0, 3),
                TooltipKey = "tooltip.item.green"
            });
            shopEntries.Add(new ShopEntryData
            {
                goldCost = RandomUtil.GetRandomInt(0, 3),
                TooltipKey = "tooltip.item.black"
            });
            shopEntries.Add(new ShopEntryData
            {
                goldCost = RandomUtil.GetRandomInt(0, 3),
                TooltipKey = "tooltip.shop.relic-chest"
            });
        }
    }
}
