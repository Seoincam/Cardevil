using UnityEngine;

namespace Cardevil.Utils
{
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
        public static bool ScreenRaycast<T>(Vector2 screenPos, LayerMask layerMask,out T @out) 
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
                if(hit.collider.TryGetComponent<T>(out T focusable))
                {
                    @out = focusable;
                    return true;
                }
            }
            @out = null;
            return false;
        }
        
    }
}