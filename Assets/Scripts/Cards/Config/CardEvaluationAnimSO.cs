using Cardevil.Attributes;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;

namespace Cardevil.Cards.Config
{
    [CreateAssetMenu(fileName = "CardEvaluationAnimSetting", menuName = "Cards/Card Evaluation Anim")]
    public class CardEvaluationAnimSO : ScriptableObject
    {
        [VisibleOnly, SerializeField, TextArea]
        private string description = "족보 선택/카드 변경 글자 애니메이션 설정 SO입니다.";

        [Header("[ Common ]")]
        [FormerlySerializedAs("s_posX")]
        [Tooltip("Sub PosX")]
        [Min(30f)] public float subPosX;
        
        [Tooltip("Main Scale Tween의 value. 이만큼 확대"), Min(1f)]
        public float mainRankingScaleValue;

        
        [Header("[ Ranking Changed ]")]
        [FormerlySerializedAs("m_RankingChangeDur"), Min(0f)]
        [Tooltip("Main Scale Tween 지속 시간. 이 시간만큼 두 번 반복.")]
        public float mainRankingChangeDur;

        [FormerlySerializedAs("s_RankingChangeDur")]
        [Tooltip("Sub PosX Tween 지속 시간."), Min(0f)]
        public float subRankingChangeDur;


        [Header("[ Step Evaluation ]")]
        [FormerlySerializedAs("mainevaDur")]
        [FormerlySerializedAs("m_evaDur")]
        [Tooltip("Main Scale Tween 지속 시간. 이 시간만큼 두 번 반복.")]
        [Min(0f)] public float mainEvaDur;
        [FormerlySerializedAs("mainevaChangeDur")]
        [FormerlySerializedAs("m_evaChangeDur")]
        [Tooltip("Main 숫자 변경 Tween 지속 시간.")]
        [Min(0f)] public float mainEvaChangeDur;

        [FormerlySerializedAs("s_evaDur")]
        [Tooltip("Sub PosX Tween 지속 시간."), Space]
        [Min(0f)] public float subEvaDur;
        [FormerlySerializedAs("s_evaEase")] [Tooltip("Sub PosX Tween Ease")]
        public Ease subEvaEase;

        [Tooltip("Sub Text가 사라질 때 사용되는 Ease")] public Ease subFadeOutEase;


        [Header("[ Evaluation Action ]")] 
        [Header("Card Visual")]
        [Min(1f)] public float cardScaleValue;
        [Min(0f)] public float cardScaleDur;
        public Ease cardScaleEase;

        [Header("[ Etc ]")] 
        [Min(0f)] public float clearTextDur;
    }
}
