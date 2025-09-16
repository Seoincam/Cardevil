using UnityEngine;
using UnityEngine.UI;

namespace Cardevil.Item
{
    public abstract class Item
    {
        public Sprite sprite;

        virtual public void IsClicked() { Debug.Log("IsClicked내부함수 구현이 안되어있습니다"); }
    }
}