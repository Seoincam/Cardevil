using Cardevil.Core;
using DG.Tweening;
using UnityEngine;

namespace Cardevil.Cards.Config.StageCard
{
    [CreateAssetMenu(menuName = "Cards/Config/Stage Card/Reroll", fileName = "Stage Card Reroll Config")]
    public class StageCardRerollConfig : ScriptableObject
    {
        [field: Header("Draw")]
        [field: SerializeField] public Ease DrawMoveEase { get; private set; } = Ease.InBack;
        [field: SerializeField] public ScalableFloat DrawFlipDuration { get; private set; } = new(0.3f);
        [field: SerializeField] public ScalableFloat DrawMoveDuration { get; private set; } = new(0.5f);

        [field: Header("Discard")]
        [field: SerializeField] public Ease DiscardMoveEase { get; private set; } = Ease.InBack;
        [field: SerializeField] public ScalableFloat DiscardFlipDuration { get; private set; } = new(0.3f);
        [field: SerializeField] public ScalableFloat DiscardMoveDuration { get; private set; } = new(0.5f);
        // ㅎㅏ이~
    }
}