using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    public static T Instance { get; protected set; }

    protected virtual void Awake()
    {
        // 씬 이동이 없을 예정이므로,
        // 일단 파괴 방지 설정 x
        Instance = this as T;
    }
}
