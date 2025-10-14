using UnityEngine;

namespace Cardevil.Shop
{
    public enum ShopItemType
    {
        None = 0,
        Potion,
        Weapon,
        Armor,
        Accessory,
        Scroll,
        Miscellaneous
    }
    [CreateAssetMenu(fileName = "ShopSettings", menuName = "ScriptableObjects/Shop/ShopSettingsSO", order = 1)]
    public class ShopSettingsSO : ScriptableObject
    {
        
    }
}