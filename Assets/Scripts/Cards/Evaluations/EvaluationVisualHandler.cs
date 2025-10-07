using Cardevil.Utils;
using DG.Tweening;
using System.Linq;
using UnityEngine;

namespace Cardevil.Cards.Evaluations
{
    public class EvaluationVisualHandler : MonoBehaviour
    {
        [SerializeField] CardEvaluationAnimSO anim;

        [Space, SerializeField] RectTransform main;
        [SerializeField] RectTransform sub;



        private EvaluationTextController _mainText;
        private EvaluationTextController _subText;

        private HandRanking _lastRanking;

        private Sequence _stepSeq;
        private Tween _mainRankingTween;
        private Tween _subRankingTween;



        void Start()
        {
            _mainText = main.GetComponentInChildren<EvaluationTextController>();
            _subText = sub.GetComponentInChildren<EvaluationTextController>();
            _mainText.UpdateText();
            _subText.UpdateText();


            Managers.Card.EvaluationEvent.OnStep += StepEvaluation;
            Managers.Card.StageCardsCtx.OnSelectsChaged += UpdateSelectedRanking;
        }

        void OnDestroy()
        {
            Managers.Card.EvaluationEvent.OnStep -= StepEvaluation;
            Managers.Card.StageCardsCtx.OnSelectsChaged -= UpdateSelectedRanking;

            _mainRankingTween?.Kill();
            _subRankingTween?.Kill();
        }



        private void UpdateSelectedRanking(HandRanking ranking)
        {
            if (ranking == _lastRanking) return;
            _lastRanking = ranking;

            if (ranking is HandRanking.None or HandRanking.High)
            {
                _mainText.UpdateText();
                _subText.UpdateText();
                return;
            }

            var data = Managers.Database.Database.HandRankingDataList
                .FirstOrDefault(i => i.Ranking == ranking);
            if (data == null) { LogEx.LogError($"Can't find HandRanking Data: {ranking}"); return; }

            // 이전 Tween 정리 및 Transform 초기화
            _mainRankingTween?.Kill();
            _subRankingTween?.Kill();
            main.localScale = Vector3.one;
            sub.anchoredPosition = new Vector3(100, 0);

            // 새 Tween
            _mainRankingTween = main.DOScale(1.2f, anim.m_RankingChangeDur)
                .SetLoops(2, LoopType.Yoyo)
                .SetAutoKill(true).SetLink(main.gameObject);
            _subRankingTween = sub.DOAnchorPosX(0, anim.s_RankingChangeDur)
                .SetAutoKill(true).SetLink(sub.gameObject);

            _mainText.UpdateText(data.DisplayName);
            _subText.UpdateText("+" + data.Value.ToString());
        }

        private void StepEvaluation(EvaluationStep step)
        {
            if (step.Effect == EvaluationAction.EvaluationEffect.Move) return;

            var oper = step.Effect == EvaluationAction.EvaluationEffect.Plus ? "+" : "x";
            _stepSeq?.Kill();
            _stepSeq = DOTween.Sequence().SetAutoKill(true).SetLink(gameObject);

            sub.anchoredPosition = new Vector3(100, 0);
            _subText.UpdateText($"{oper}{step.Value}");

            _stepSeq.Append(sub.DOAnchorPosX(0, anim.s_evaDur).SetEase(anim.s_evaEase));
            _stepSeq.Append(main.DOScale(1.1f, anim.m_evaDur));
            _stepSeq.Append(
                DOTween.To(
                    () => step.Before,
                    v => _mainText.UpdateText(v.ToString("0")),
                    step.After,
                    anim.m_evaChangeDur
                )
            );
            _stepSeq.Append(main.DOScale(1f, anim.m_evaDur));
        }
    }
}
