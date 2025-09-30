using UnityEngine;
using Cardevil.Item;

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
       
    }

    public override Item DeepClone()
    {
        return MemberwiseClone() as Item;
    }
}
