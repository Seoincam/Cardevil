using UnityEngine;
using Cardevil.Item;
public class Heal : Item
{
    int healAmount;
    public Heal(int amount)
    {
        healAmount = amount;
    }

    public Heal()
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
