using Cardevil.Cards.Data;
using Cardevil.Cards.ScriptableObjects;
using Cardevil.Core;
using Cardevil.Core.Bootstrap;
using Cardevil.Utils;
using Cysharp.Threading.Tasks;
using Database;
using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using NotImplementedException = System.NotImplementedException;

namespace Cardevil.Cards.Evaluations
{
    /*
     * 1. sub 글자가 사라지기 (ok)
     * 2. value에 따라 main 텍스트 올라가는 시간이 바뀌기 (ok)
     * 3. 점점 빨라지기
     */
    public class EvaluationView : MonoBehaviour
    {
        [SerializeField] private CardEvaluationAnimSO animSO;
        [SerializeField] private RectTransform main;

        private TextAnimator _mainText;
        private static readonly Queue<TextAnimator> SubPool = new();
        private static readonly Queue<TextAnimator> RegisteredSubs = new();

        private HandRanking _lastRanking;
        private float _lastStepY;

        private Sequence _stepSeq;
        private Tween _mainRankingTween;
        private Tween _subRankingTween;

        private void Awake()
        {
            _mainText = main.GetComponentInChildren<TextAnimator>();
        }

        private void OnDestroy()
        {
            // if (_event != null)
            // _event.OnStep -= StepEvaluation;

            _mainRankingTween?.Kill();
            _subRankingTween?.Kill();
        }
        
        public async UniTask Clear()
        {
            _prevDamage = 0f;

            var waitAll = new List<UniTask>(SubPool.Count + 1);
            waitAll.Add(ClearTextAsync(_mainText));
            foreach (var sub in SubPool)
                waitAll.Add(ClearTextAsync(sub));

            await waitAll;
        }
        
        public void UpdateHandRankingText(HandRanking ranking)
        {
            if (ranking == _lastRanking) return;
            _lastRanking = ranking;

            var sub = GetSub();
            if (ranking is HandRanking.None)
            {
                ClearTextAsync(_mainText).Forget();
                ClearTextAsync(sub).Forget();
                SubPool.Enqueue(sub);
                return;
            }

            var data = CardevilCore.Instance.Database.Database.HandRankingDataList
                .FirstOrDefault(i => i.Ranking == ranking);
            if (data == null) { LogEx.LogError($"Can't find HandRanking Data: {ranking}"); return; }

            var subRect= sub.transform.parent.GetComponent<RectTransform>();
            
            // 이전 Tween 정리 및 Transform 초기화
            _mainRankingTween?.Kill();
            _subRankingTween?.Kill();
            main.localScale = Vector3.one;
            subRect.anchoredPosition = new Vector3(animSO.subPosX, 0);

            // 새 Tween
            _mainRankingTween = main.DOScale(animSO.mainRankingScaleValue, animSO.mainRankingChangeDur)
                .SetLoops(2, LoopType.Yoyo)
                .SetAutoKill(true).SetLink(main.gameObject);
            _subRankingTween = subRect.DOAnchorPosX(0, animSO.subRankingChangeDur)
                .SetAutoKill(true).SetLink(sub.gameObject);

            _mainText.UpdateText(data.DisplayName);
            sub.UpdateText("+" + data.Value.ToString());
            
            SubPool.Enqueue(sub);
        }

        // 스텝을 등록함
        public void RegisterStep(EvaluationStep s)
        {
            if (s.StepType is EvaluationStep.Type.None or EvaluationStep.Type.Move) 
                return;
            
            var sub = GetSub();
            var rect = sub.transform.parent.GetComponent<RectTransform>();
            
            _stepSeq?.Kill();
            _stepSeq = DOTween.Sequence().SetAutoKill(true).SetLink(sub.gameObject);
            
            rect.anchoredPosition= new Vector3(animSO.subPosX * 1.5f, _lastStepY);
            string oper = s.StepType == EvaluationStep.Type.Plus ? "+" : "x"; // TODO: 더 제대로 나눠야함.
            sub.UpdateText($"{oper}{s.Value}");
            
            _stepSeq
                .Append(rect.DOAnchorPosX(animSO.subPosX, animSO.subEvaDur).SetEase(animSO.subEvaEase));
            
            RegisteredSubs.Enqueue(sub);
            _lastStepY += 100f;
        }
        
        private float _prevDamage; // 데미지 캐시 
        
        // 등록된 스텝을 일괄 실행
        public async UniTask DoStep(float totalDamage)
        {
            if (RegisteredSubs.Count == 0)
                return;
            _mainText.UpdateText(_prevDamage.ToString());

            // 1. sub Text 이동
            var subs = new List<TextAnimator>(RegisteredSubs.Count);
            while(RegisteredSubs.Count > 0)
                subs.Add(RegisteredSubs.Dequeue());
            
            var waitAll = new List<UniTask>(subs.Count);
            foreach (var sub in subs)
            {
                var rect = sub.transform.parent.GetComponent<RectTransform>();
                var seq = DOTween.Sequence()
                    .Append(rect.DOAnchorPosX(0, animSO.subEvaDur)
                        .SetEase(animSO.subEvaEase))
                    .Join(DOTween.To(
                        () => 1f,
                        sub.SetAlpha,
                        0f,
                        animSO.subEvaDur)
                        .SetEase(animSO.subFadeOutEase))
                    .SetAutoKill()
                    .SetRecyclable()
                    .SetLink(gameObject);

                waitAll.Add(seq.AwaitForComplete());
            }
            await UniTask.WhenAll(waitAll);
            
            // 2. main Text에 total damage 반영
            float dur = ((totalDamage - _prevDamage) / 10f + 1) * animSO.mainEvaChangeDur; // 데미지에 따라 시간 늘어나도록
            var mainSeq = DOTween.Sequence()
                .Append(main.DOScale(animSO.mainRankingScaleValue, animSO.mainEvaDur)
                    .SetLoops(2, LoopType.Yoyo))
                .Join(DOTween.To(
                    () => _prevDamage,
                    v => _mainText.UpdateText(v.ToString("0")),
                    totalDamage,
                    dur))
                .SetAutoKill()
                .SetRecyclable()
                .SetLink(gameObject);
           await mainSeq;
           
            _prevDamage = totalDamage;
            _lastStepY = 0f;

            // 3. 다시 pool에 sub Text 반환
            foreach (var sub in subs)
            {
                sub.ClearText();
                SubPool.Enqueue(sub);
            }
        }

        private TextAnimator GetSub()
        {
            if (SubPool.Count > 0)
                return SubPool.Dequeue();
            
            const string path = "UI/CardUI/Evaluation View Sub";
            var go = AssetUtil.Instantiate(path, transform);
            if (!go)
            {
                LogEx.LogError($"Can't find Evaluation View Sub: {path}");
                return null;
            }
            
            var sub = go.GetComponentInChildren<TextAnimator>();
            return sub;
        }

        private async UniTask ClearTextAsync(TextAnimator text)
        {
            await DOTween.Sequence()
                .Append(DOTween.To(
                    () => 1f,
                    text.SetAlpha,
                    0f,
                    animSO.clearTextDur));
            text.ClearText();
        }
    }
}
