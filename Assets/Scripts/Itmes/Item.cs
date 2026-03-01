using Cardevil.Core;
using UnityEngine;
using UnityEngine.UI;
using Database.Generated;

namespace Cardevil.Item
{
    public abstract class Item : IDeepClonable<Item>, IShallowClonable<Item>
    {
        public string itemName;
        public Sprite sprite;
        public Define.RareType rareType;
        public MachineReward macinRewardData;

        virtual public void OnClicked() { Debug.Log("IsClicked내부함수 구현이 안되어있습니다"); }
        virtual public void GoItemSetting() { Debug.Log("GoItemSetting내부함수 구현이 되어있지않습니다"); }
        
        public abstract Item DeepClone();
        public Item ShallowClone()
        {
            return MemberwiseClone() as Item;
        }
    }
}