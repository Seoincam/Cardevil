using UnityEngine;

namespace Cardevil.Cards.ScriptableObjects
{
    [CreateAssetMenu(fileName = "DeckRemainViewAnimSetting", menuName = "Cards/Deck Remain View Anim Setting")]
    public class DeckRemainViewAnimSetting : ScriptableObject
    {
        public enum AnimType { Pop, FadeInUp }

        [Tooltip("애니메이션 모드")] 
        public AnimType animType;
        
        [Tooltip("카드 간 딜레이(sec)"), Range(0, .15f)] 
        public float delay = .05f;

        [Tooltip("개별 카드 애니메이션 시간(sec)"), Range(0, .6f)] 
        public float duration = .4f;
        
        
        [Tooltip("딜레이 계산에 쓰는 기울기"), Range(0, 2)] 
        public float angle = .45f;
        
        [Tooltip("시작 Y offset"), Min(0f)]
        public float startY;
        
        [Tooltip("패널 열리고 대기 시간"), Range(0, 1f)]
        public float startInterval = .15f;
    }
}