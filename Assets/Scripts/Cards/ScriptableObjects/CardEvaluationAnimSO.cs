using Cardevil.Attributes;
using DG.Tweening;
using UnityEngine;

namespace Cardevil
{
    [CreateAssetMenu(fileName = "CardEvaluationAnimSetting", menuName = "Cards/Card Evaluation Anim")]
    public class CardEvaluationAnimSO : ScriptableObject
    {
        [VisibleOnly, SerializeField, TextArea]
        string description = "족보 선택/카드 변경 글자 애니메이션 설정 SO입니다.";


        [Header("Ranking Changed")]
        [Tooltip("Main Scale Tween 지속 시간. 이 시간만큼 두 번 반복.")]
        [Min(0f)] public float m_RankingChangeDur;

        [Tooltip("Sub PosX Tween 지속 시간.")]
        [Min(0f)] public float s_RankingChangeDur;


        [Header("Step Evaluation")]
        [Tooltip("Main Scale Tween 지속 시간. 이 시간만큼 두 번 반복.")]
        [Min(0f)] public float m_evaDur;
        [Tooltip("Main 숫자 변경 Tween 지속 시간.")]
        [Min(0f)] public float m_evaChangeDur;

        [Tooltip("Sub PosX Tween 지속 시간."), Space]
        [Min(0f)] public float s_evaDur;
        [Tooltip("Sub PosX Tween Ease")]
        public Ease s_evaEase;
    }
}
