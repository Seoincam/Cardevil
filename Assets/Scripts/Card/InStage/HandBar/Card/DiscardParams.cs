using DG.Tweening;
using UnityEngine;

namespace Cardevil.Card.InStage
{
    [CreateAssetMenu(menuName = "NewCards/Config/HandCard/DiscardParams")]
    public class DiscardParams : ScriptableObject
    {
        [field: SerializeField] public float Speed { get; private set; } = 10f;
        
        [field: Header("Scale")]
        [field: SerializeField] public float TargetScale { get; private set; } = 0.2f;
        [field: SerializeField] public Ease ScaleEase { get; private set; } = Ease.Unset;

        [field: Header("Rotation")]
        [field: SerializeField] public Vector3 Rotation { get; private set; } = new(0f, 0f, 260f);
        [field: SerializeField] public Ease RotationEase { get; private set; } = Ease.Unset;

        [field: Header("Jump")]
        [field: SerializeField] public float JumpPower { get; private set; } = 3f;
        [field: SerializeField] public Vector2 JumpPowerRandomRange { get; private set; } = new(1f, 2f);
        [field: SerializeField] public Ease JumpEase { get; private set; } = Ease.InOutFlash;

        [field: Header("Fade")]
        [field: SerializeField] public Ease FadeEase { get; private set; } = Ease.Unset;
    }
}