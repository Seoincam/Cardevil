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
        Debug.Log(this.itemName);
        Managers.UI.ClosePopUpUI();
    }
    public override Item DeepClone()
    {
        return MemberwiseClone() as Item;
    }
}
