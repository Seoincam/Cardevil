using UnityEngine;
using UnityEngine.UI;

namespace Cardevil.Item
{
    public abstract class Item
    {
        public string itemName;
        public Sprite sprite;
        public Define.RareType type;

        virtual public void IsClicked() { Debug.Log("IsClicked내부함수 구현이 안되어있습니다"); }
        virtual public void IsItemSetting() { Debug.Log("IsItemSetting내부함수 구현이 되어있지않습니다"); }
    }
}