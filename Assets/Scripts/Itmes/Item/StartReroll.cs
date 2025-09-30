using UnityEngine;
using Cardevil.Item;

public class StartReroll : Item
{
    int rerollAmount;
    public StartReroll(int amount)
    {
        rerollAmount = amount;
    }
    public StartReroll()
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
