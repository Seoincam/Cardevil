using UnityEngine;

namespace Cardevil.Utils
{
    /// <summary>
    /// 분류하기 힘든 유틸리티 함수들을 모아놓은 static 클래스.
    /// </summary>
    public static class Util
    {
     
        
        /// <summary>
        /// 화면 좌표계를 기준으로 Raycast를 통해 Type T의 컴포넌트를 가진 오브젝트를 찾음
        /// 거리와 상관없음
        /// </summary>
        /// <param name="screenPos">화면 좌표</param>
        /// <param name="layerMask">레이어 마스큰</param>
        /// <param name="out">출력</param>
        /// <typeparam name="T">받아올 컴포넌트</typeparam>
        /// <returns>성공여부</returns>
        public static bool ScreenRaycast<T>(Vector2 screenPos, LayerMask layerMask, out T @out)
        where T : class
        {
            return ScreenRaycast<T>(screenPos, layerMask, Camera.main, out @out);
        }

        /// <summary>
        /// 화면 좌표계를 기준으로 Raycast를 통해 Type T의 컴포넌트를 가진 오브젝트를 찾음
        /// 거리와 상관없음
        /// </summary>
        /// <param name="screenPos">화면 좌표</param>
        /// <param name="layerMask">레이어 마스크</param>
        /// <param name="camera">카메라</param>
        /// <param name="out">출력</param>
        /// <param name="maxHits">최대 히트 수</param>
        /// <typeparam name="T">받아올 컴포넌트</typeparam>
        /// <returns>성공 여부</returns>
        public static bool ScreenRaycast<T>(Vector2 screenPos, LayerMask layerMask, Camera camera, out T @out, int maxHits = 10)
        where T : class
        {
            var ray = camera.ScreenPointToRay(screenPos);

            // Debug.DrawRay(ray.origin, ray.direction * 100, Color.red, 2f);
            var hits = new RaycastHit[maxHits];
            var num = Physics.RaycastNonAlloc(ray, hits, 100f, layerMask.value);
            // Debug.Log($"Raycast hit {num} objects");
            for (int i = 0; i < num; i++)
            {
                var hit = hits[i];
                if (hit.collider.TryGetComponent<T>(out T focusable))
                {
                    @out = focusable;
                    return true;
                }
            }
            @out = null;
            return false;
        }

        public static T GetOrAddComponent<T>(GameObject go) where T : UnityEngine.Component
        {
            T component = go.GetComponent<T>();
            if (component == null)
            {
                component = go.AddComponent<T>();
            }
            return component;
        }
        public static GameObject FindChild(GameObject go, string name = null, bool recursive = false) // ���ӿ�����Ʈ ��ü�� ��ȯ
        {
            Transform transfrom = FindChild<Transform>(go, name, recursive);
            if (transfrom == null)
            {
                return null;
            }
            return transfrom.gameObject;
        }

        public static T FindChild<T>(GameObject go, string name = null, bool recursive = false) where T : UnityEngine.Object // ���� ������Ʈ�� �ڽĵ��� T�� ������Ʈ ��ȯ
        {
            if (go == null)
                return null;
            if (recursive == false)
            {
                for (int i = 0; i < go.transform.childCount; i++)
                {
                    Transform transform = go.transform.GetChild(i);
                    if (string.IsNullOrEmpty(name) || transform.name == name)
                    {
                        T component = transform.GetComponent<T>();
                        if (component != null)
                        {
                            return component;
                        }
                    }
                }

            }
            else
            {
                foreach (T component in go.GetComponentsInChildren<T>())
                {
                    if (string.IsNullOrEmpty(name) || component.name == name)
                    {
                        return component;
                    }

                }
            }
            return null;
        }
    }
}