using UnityEngine;
using Cardevil.Item;

public class StartReroll : Item
{
    int rerollAmount;
    public StartReroll(int amount)
    {
        rerollAmount = amount;
    }
    public override void IsClicked()
    {
       
    }
}
