using DG.Tweening;
using UnityEngine;

namespace Cardevil.Cards.InStage.NCard
{
    public class CardChangeValueFlipSetting : ScriptableObject
    {
        [field: Tooltip("값이 변경될 때 회전 횟수. 뒤-앞 회전을 1회로 침.")]
        [field: SerializeField] 
        public int FlipCount { get; private set; }
        
        [field: Tooltip("실제 값이 변경되는 인덱스. 1일 경우, 첫 회전 중 변경됨.")]
        [field: SerializeField] 
        public int ValueChangeIndex { get; private set; }
        
        [field: Tooltip("앞 -> 뒤 플립 시간.")]
        [field: SerializeField]
        public float FrontToBackDuration { get; private set; }
        
        [field: Tooltip("뒤 -> 앞 플립 시간.")]
        [field: SerializeField]
        public float BackToFrontDuration { get; private set; }
        
        [field: SerializeField]
        public Ease FlipEase { get; private set; }
    }
}