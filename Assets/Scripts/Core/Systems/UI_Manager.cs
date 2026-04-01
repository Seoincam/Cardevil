using Cardevil.Core.Utils;
using Cardevil.UI.PopUp;
using System.Collections.Generic;
using UnityEngine;

namespace Cardevil.Core.Systems
{
    public class UI_Manager
    {
        int _order = 10;

        Stack<UI_Popup> _popUpStack = new Stack<UI_Popup>();

        UI_Scene _sceneUI = null;
        public GameObject Root
        {
            get
            {
                GameObject root = GameObject.Find("@UI_root");
                if (root == null)
                {
                    root = new GameObject { name = "@UI_root" };
                }
                return root;
            }
        }

        public void SetCanvas(GameObject go, bool sort = true)
        {
            Canvas canvas = Util.GetOrAddComponent<Canvas>(go);
            //canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = Camera.main;
            canvas.overrideSorting = true;

            if (sort)
            {
                canvas.sortingOrder = _order;
                _order++;
            }
            else
            {
                canvas.sortingOrder = 5;
            }
        }
    
        public void SetCanvasMost(GameObject go) // 캔버스 SortOrder을 가장 위로 올려주는 함수
        {
            Canvas canvas = Util.GetOrAddComponent<Canvas>(go);
            //canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = Camera.main;
            canvas.overrideSorting = true;

 
            canvas.sortingOrder = 30000;
        }

        public T ShowAnyUI<T>(string name = null) where T : UI_Base
        {
            if (string.IsNullOrEmpty(name))
            {
                name = typeof(T).Name;
            }
            GameObject go = AssetUtil.Instantiate($"UI/{name}");
            T anyUI = Util.GetOrAddComponent<T>(go);
            return anyUI;
        }
        public T ShowPopUpUI<T>(string name = null) where T : UI_Popup
        {
            if (string.IsNullOrEmpty(name))
            {
                name = typeof(T).Name;
            }
            GameObject go = AssetUtil.Instantiate($"UI/PopUP/{name}");
            T popUp = Util.GetOrAddComponent<T>(go);
            _popUpStack.Push(popUp);
            go.transform.SetParent(Root.transform);
            return popUp;
        }
        public T ShowSceneUI<T>(string name = null) where T : UI_Scene
        {
            if (string.IsNullOrEmpty(name))
            {
                name = typeof(T).Name;
            }
            GameObject go = AssetUtil.Instantiate($"UI/Scene/{name}");
            T sceneUI = Util.GetOrAddComponent<T>(go);
            _sceneUI = sceneUI;
            go.transform.SetParent(Root.transform);
            return sceneUI;
        }

        public void ClosePopUpUI()
        {
            if (_popUpStack.Count == 0)
            {
                return;
            }
            UI_Popup popUP = _popUpStack.Pop();
            Debug.Log($"ClosePopUpUI : {popUP}");
            AssetUtil.Destroy(popUP.gameObject);
            popUP = null;

            _order--;
        }
        public void ClosePopUpUI(UI_Popup popUp)
        {
            if (_popUpStack.Count == 0)
            {
                return;
            }
            if (_popUpStack.Peek() != popUp)
            {
                Debug.Log("Close PopUp Failed");
                return;
            }
            ClosePopUpUI();
        }
        public void CloseAllPopUPUI()
        {
            while (_popUpStack.Count > 0)
            {
                ClosePopUpUI();
            }
            Time.timeScale = 1;
        }
        public void Clear()
        {
            CloseAllPopUPUI();
            _sceneUI = null;
        }
    }
}
