using Cardevil.DataStructure.Serializables;
using DG.Tweening;
using UnityEngine;

namespace Cardevil.Cards.ScriptableObjects
{
    [CreateAssetMenu(fileName = "ValueSelectionViewAnimSetting", menuName = "Cards/Value Selection View Anim Setting")]
    public class ValueSelectionViewAnimSetting : ScriptableObject
    {
        [Tooltip("열리는 위치")]
        public Vector2 openPosition = new(0, -125);

        [Tooltip("개수에 따른 Bar Width")]
        public SerializableDictionary<int, float> frameWidths = new()
        {
            { 2, 391 }, { 3, 542 }, { 4, 691 }, { 9, 1300 }
        };

        [Space]
        [Tooltip("Bar 페이드 인 시간"), Range(0, 1)]
        public float barFadeInDuration = .4f;

        [Tooltip("카드 페이드 인 + Y Up 시간"), Range(0, .6f)]
        public float cardFadeInUpDuration = .2f;

        [Tooltip("각 카드마다 소환 간격"), Range(0, 0.1f)]
        public float cardInterval = .04f;

        [Space, Header("드래그 카드 페이드 설정")] 
        public float draggedCardFadeDuration = .2f;
        public Ease draggedCardFadeEase = Ease.InCubic;
    }
}