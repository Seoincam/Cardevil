using Cardevil.Core.Systems;
using UnityEngine;

namespace Cardevil.Gameplay.Items
{
    public class ExactUpgrade : Item
    {
        int exactUpgradeAmount;
    
        public ExactUpgrade(int amount)
        {
            exactUpgradeAmount = amount;
        }
    
        public ExactUpgrade()
        {

        }
        public override void OnClicked()
        {
            Debug.Log(this.itemName);
            Managers.UI.ClosePopUpUI();
        }

        public override Item DeepClone()
        {
            return MemberwiseClone() as Item;
        }
    }
}
