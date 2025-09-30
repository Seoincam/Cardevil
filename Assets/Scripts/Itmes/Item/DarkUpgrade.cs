using UnityEngine;
using Cardevil.Item;

public class DarkUprade : Item
{
    public int value;

    public DarkUprade()
    {

    }
    public DarkUprade(int val)
    {
        value = val;
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
