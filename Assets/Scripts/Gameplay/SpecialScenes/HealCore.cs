using Cardevil.Core;
using Cardevil.Core.Utils;
using Database.Generated;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Cardevil.Gameplay.SpecialScenes
{
    public class HealCore : SpecialSceneCore
    {
        public override string TestTitle => "Heal";
        public override string TestDescription => $"Floor {Context.floor} recovery point.\nClosing this scene clears the heal node and returns to the map.";
        public override string TestConfirmLabel => "Recover";
        public override Color TestAccentColor => new(0.22f, 0.67f, 0.58f, 0.96f);

        private int _goldAmount = 0;
        private int _selectedHealIndex = 0;
        private int _selectedHealAmount = 0;

        
        public int GoldAmount => _goldAmount;
        public int SelectedHealIndex => _selectedHealIndex;
        public int SelectedHealAmount => _selectedHealAmount;
        public int MinHealAmount { get; private set; }
        public int MaxHealAmount { get; private set; }
        
        public override void Initialize(GameFlowManager.SpecialSceneEnterContext context)
        {
            base.Initialize(context);

            Heal healData = Context.node.HealData;
            if (healData == null)
            {
                Debug.LogError("Heal data is null for this node.");
                return;
            }

            _goldAmount = healData.Gold;
            _selectedHealIndex = RandomUtil.WeightedRandomIndex(healData.HealProbabillity);
            List<int> healAmounts = healData.HealAmount;
            if (healAmounts == null || healAmounts.Count == 0)
            {
                Debug.LogError("Heal amount data is empty for this node.");
                MinHealAmount = 0;
                MaxHealAmount = 0;
                _selectedHealAmount = 0;
                return;
            }

            MinHealAmount = healAmounts.Min();
            MaxHealAmount = healAmounts.Max();
            
            _selectedHealAmount = healAmounts[_selectedHealIndex];
        }

        public void ApplyGoldChoice(PlayerStatus playerStatus)
        {
            if (playerStatus == null)
            {
                Debug.LogError("PlayerStatus is null. Gold choice cannot be applied.");
                return;
            }

            playerStatus.ModifyBaseValue(StatType.Gold, GoldAmount);
        }

        public void ApplyHealChoice(PlayerStatus playerStatus)
        {
            if (playerStatus == null)
            {
                Debug.LogError("PlayerStatus is null. Heal choice cannot be applied.");
                return;
            }

            playerStatus.Heal(SelectedHealAmount);
        }
    }
}
