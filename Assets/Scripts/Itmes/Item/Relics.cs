using UnityEngine;
using Cardevil.Item;

public class Relics : Item
{
   public override void OnClicked()
   {
       
   }
   
   public override Item DeepClone()
   {
       return MemberwiseClone() as Item;
   }
}
