using Cardevil.Utils;
using Database.Generated;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;

namespace Cardevil.Cards.Evaluations
{
    public class EvaluationVisualHandler : MonoBehaviour
    {
        [SerializeField] RectTransform main;
        [SerializeField] RectTransform sub;
        [Space]
        [SerializeField] float mainDur;
        [SerializeField] float mainToDur;
        [SerializeField] Ease mainEase;
        [SerializeField] float subDur;
        [SerializeField] Ease subEase;

        private EvaluationTextController _mainText;
        private EvaluationTextController _subText;

        void Start()
        {
            _mainText = main.GetComponentInChildren<EvaluationTextController>();
            _subText = sub.GetComponentInChildren<EvaluationTextController>();
            _mainText.UpdateText();
            _subText.UpdateText();


            Managers.Card.EvaluationEvent.OnStep += StepEvaluation;
            Managers.Card.StageCardsCtx.OnSelectsChaged += UpdateSelectedRanking;
        }

        private void UpdateSelectedRanking(HandRankingData data)
        {
            if (data == null)
            {
                _mainText.UpdateText();
                _subText.UpdateText();
                return;
            }

            _mainText.UpdateText(data.DisplayName);
            _subText.UpdateText("+" + data.Value.ToString());
        }

        private void StepEvaluation(EvaluationStep step)
        {
            if (step.Effect == EvaluationAction.EvaluationEffect.Move) return;

            var oper = step.Effect == EvaluationAction.EvaluationEffect.Plus ? "+" : "x";
            var seq = DOTween.Sequence();

            sub.anchoredPosition = new Vector3(100, 0);
            _subText.UpdateText(oper + step.Value);

            seq.Append(sub.DOAnchorPosX(0, subDur)
                .SetEase(subEase));
            seq.Append(main.DOScale(1.1f, mainDur));
            seq.Append(
                DOTween.To(
                    () => step.Before,
                    value => _mainText.UpdateText(value.ToString("0")),
                    step.After,
                    mainToDur
                ).SetEase(mainEase)
            );
            seq.Append(main.DOScale(1f, mainDur));
        }
    }
}
