using UnityEngine;
using Cardevil.Item;


public class Relics : Item
{
   public override void OnClicked()
   {
        Debug.Log(this.itemName);
        Managers.UI.ClosePopUpUI();
    }
   
   public override Item DeepClone()
   {
       return MemberwiseClone() as Item;
   }
   
   public virtual void GetRelic()
    {

    }
}
