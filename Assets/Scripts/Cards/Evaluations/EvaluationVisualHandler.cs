using Cardevil.Utils;
using DG.Tweening;
using System.Linq;
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

        private HandRanking cachedRanking;

        private Tween mainRankingTween;
        private Tween subRankingTween;

        void Start()
        {
            _mainText = main.GetComponentInChildren<EvaluationTextController>();
            _subText = sub.GetComponentInChildren<EvaluationTextController>();
            _mainText.UpdateText();
            _subText.UpdateText();


            Managers.Card.EvaluationEvent.OnStep += StepEvaluation;
            Managers.Card.StageCardsCtx.OnSelectsChaged += UpdateSelectedRanking;
        }

        private void UpdateSelectedRanking(HandRanking ranking)
        {
            if (ranking == cachedRanking) return;
            cachedRanking = ranking;

            if (ranking == HandRanking.None || ranking == HandRanking.High)
            {
                _mainText.UpdateText();
                _subText.UpdateText();
                return;
            }

            var data = Managers.Database.Database.HandRankingDataList.FirstOrDefault(i => i.Ranking == ranking);
            if (data == null)
            {
                LogEx.LogError($"Can't find HandRanking Data: {ranking}");
                return;
            }

            mainRankingTween?.Kill();
            subRankingTween?.Kill();

            sub.anchoredPosition = new Vector3(100, 0);
            mainRankingTween = main.DOScale(1.2f, mainDur).SetLoops(2, LoopType.Yoyo);
            subRankingTween = sub.DOAnchorPosX(0, subDur);

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
