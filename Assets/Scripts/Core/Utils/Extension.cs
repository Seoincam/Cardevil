using Cardevil.UI.PopUp;
using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Cardevil.Core.Utils
{
    public static class Extension
    {
        public static void AddUIEvent(this GameObject go, Action<PointerEventData> action, Define.UIEvent type = Define.UIEvent.Click)
        {
            UI_Base.AddUIEvent(go, action, type);
        }
        public static T GetOrAddComponent<T>(this GameObject go) where T : UnityEngine.Component
        {
            return Util.GetOrAddComponent<T>(go);
        }
    }
}
