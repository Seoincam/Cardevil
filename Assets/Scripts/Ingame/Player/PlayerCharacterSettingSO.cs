using DG.Tweening;
using UnityEngine;

namespace Cardevil.Ingame.Player
{
    [CreateAssetMenu(fileName = "PlayerSettingSO", menuName = "Cardevil/Player/Player Setting SO")]
    public class PlayerCharacterSettingSO : ScriptableObject
    {
        [Header("Movement Settings")]
        [SerializeField,Tooltip("이동 속도")] public float moveSpeed = 5f;
        [Header("Fall Animation Settings")]
        [SerializeField,Tooltip("코요테 시간")] public float coyoteTime = 0.2f;
        [SerializeField,Tooltip("떨어지는 높이")] public float fallHeight = 5f;
        [SerializeField] public float fallDuration = 1f;
        [SerializeField,Tooltip("떨어지는 속도 Ease") ] public Ease fallEase = Ease.InQuad;
        [SerializeField,Range(0.0f,1.0f),Tooltip("떨어질 때 페이드 아웃이 시작되는 비율")] public float fallFadeStartRatio = 0.5f;
        [SerializeField,Range(0.0f,1.0f),Tooltip("다시 떨어질때 페이드 인이 끝나는 비율")] public float fallFadeEndRatio = 0.8f;
        [SerializeField] public Ease fallFadeEase = Ease.InQuad;
    }
}