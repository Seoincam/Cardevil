using Cardevil.Core.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Cardevil.UI.PopUp
{
    public abstract class UI_Base : MonoBehaviour
    {
        Dictionary<Type, UnityEngine.Object[]> _objects = new Dictionary<Type, UnityEngine.Object[]>();

        public abstract void Init();
        protected void Bind<T>(Type type) where T : UnityEngine.Object // Ÿ�Կ� �´� �������� ���� _objects �迭�� ���ε�
        {
            string[] names = Enum.GetNames(type);
            UnityEngine.Object[] objects = new UnityEngine.Object[names.Length];
            _objects.Add(typeof(T), objects);

            for (int i = 0; i < names.Length; i++)
            {
                if (typeof(T) == typeof(GameObject))
                {
             
                    objects[i] = Util.FindChild(gameObject, names[i], true);
                }
                else
                {
                    objects[i] = Util.FindChild<T>(gameObject, names[i], true);
                }
                if (objects[i] == null)
                {
                    Debug.Log($"Failed to bind {names[i]}");
                }
            }
        }

    
        protected T Get<T>(Enum type) where T : UnityEngine.Object => Get<T>(Convert.ToInt32(type)); // 최적화에 소폭 악영향. 어차피 몇번 안불리므로 괜찮을듯

        protected T Get<T>(int index) where T : UnityEngine.Object //���ε� �� _objects �迭���� ���ϴ� �� Get�ؿ�
        {
            UnityEngine.Object[] objects = null;
            if (_objects.TryGetValue(typeof(T), out objects) == false)
            {
                return null;
            }
            return objects[index] as T;
        }

        protected Text GetText(int index) { return Get<Text>(index); }
        protected Button GetButton(int index) { return Get<Button>(index); }
        protected Image GetImage(int index) { return Get<Image>(index); }
        protected Slider GetSlider(int index) { return Get<Slider>(index); }

        public static void AddUIEvent(GameObject go, Action<PointerEventData> action, Define.UIEvent type = Define.UIEvent.Click)
        {
            UI_EventHandler _event = Util.GetOrAddComponent<UI_EventHandler>(go);
            switch (type)
            {
                case Define.UIEvent.Click:
                    _event.OnClickHandler -= action;
                    _event.OnClickHandler += action;
                    break;
                case Define.UIEvent.BeginDrag:
                    _event.BeginDragHandler -= action;
                    _event.BeginDragHandler += action;
                    break;
                case Define.UIEvent.Drag:
                    _event.DragHandler -= action;
                    _event.DragHandler += action;
                    break;
                case Define.UIEvent.DragEnd:
                    _event.DragEndHandler -= action;
                    _event.DragEndHandler += action;
                    break;
                case Define.UIEvent.PointerUP:
                    _event.OnPointerUpHandler -= action;
                    _event.OnPointerUpHandler += action;
                    break;
                case Define.UIEvent.PointerDown:
                    _event.OnPointerDownHandler -= action;
                    _event.OnPointerDownHandler += action;
                    break;
            }

        }

        public void SetResolution()
        {
            int setWidth = 1920; // 사용자 설정 너비
            int setHeight = 1080; // 사용자 설정 높이

            int deviceWidth = Screen.width; // 기기 너비 저장
            int deviceHeight = Screen.height; // 기기 높이 저장

            Screen.SetResolution(setWidth, (int)(((float)deviceHeight / deviceWidth) * setWidth), true); // SetResolution 함수 제대로 사용하기

            if ((float)setWidth / setHeight < (float)deviceWidth / deviceHeight) // 기기의 해상도 비가 더 큰 경우
            {
                float newWidth = ((float)setWidth / setHeight) / ((float)deviceWidth / deviceHeight); // 새로운 너비
                //Camera.main.rect = new Rect((1f - newWidth) / 2f, 0f, newWidth, 1f); // 새로운 Rect 적용
            }
            else // 게임의 해상도 비가 더 큰 경우
            {
                float newHeight = ((float)deviceWidth / deviceHeight) / ((float)setWidth / setHeight); // 새로운 높이
                //Camera.main.rect = new Rect(0f, (1f - newHeight) / 2f, 1f, newHeight); // 새로운 Rect 적용
            }

            CanvasScaler canvasScaler = GetComponent<CanvasScaler>(); // CanvasScaler 컴포넌트 가져오기
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; // uiScaleMode 설정
            canvasScaler.referenceResolution = new Vector2(setWidth, setHeight); // referenceResolution 설정
            canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight; // screenMatchMode 설정
            canvasScaler.matchWidthOrHeight = 0; // matchWidthOrHeight 설정
            canvasScaler.referencePixelsPerUnit = 100; // referencePixelsPerUnit 설정
        }
    }
}
