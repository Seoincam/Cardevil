using Cardevil.Core;
using DG.Tweening;
using UnityEngine;

namespace Cardevil.Cards.Config.StageCard
{
    [CreateAssetMenu(menuName = "Cards/Config/Stage Card/Default", fileName = "Stage Card Default Config")]
    public class StageCardDefaultConfig : ScriptableObject
    {
        [field: Header("Follow")]
        [field: SerializeField] public float LerpToSlotSpeed { get; private set; } = 14f;
        [field: SerializeField] public float DragFollowSpeed { get; private set; } = 14f;
        [field: SerializeField] public float CurveRotateSpeed { get; private set; } = 20f;

        [field: Header("Draw / Discard")]
        [field: SerializeField]
        public ScalableFloat FlipDuration { get; private set; } = new(0.4f);
        [field: SerializeField] public Ease FlipEase { get; private set; } = Ease.InOutSine;
        [field: SerializeField] public ScalableFloat DrawMoveSpeed { get; private set; } = new(14f);
        [field: SerializeField] public Ease DrawMoveEase { get; private set; } = Ease.InExpo;
    }
}