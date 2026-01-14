using Cardevil.Cards.Data;
using Cardevil.Cards.ScriptableObjects;
using Cardevil.Core.Bootstrap;
using Cardevil.Utils;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Cardevil.Cards.Evaluations
{
    public class EvaluationView : MonoBehaviour
    {
        [SerializeField] private CardEvaluationAnimSO setting;
        [SerializeField] private RectTransform rect;
        
        public static EvaluationView Current { get; private set; }

        private HandRanking _lastRanking;
        private Tween _mainRankingTween;
        private Tween _subRankingTween;
        
        private TextAnimator _mainText;
        private TextAnimator _subText;
        // TODO: Pooling 구현해야할 듯
        
        private float _cachedDamage;

        private void Awake()
        {
            _mainText = rect.GetComponentInChildren<TextAnimator>();
            
            if (Current && Current != this)
                Destroy(Current);
            Current = this;
        }

        private void OnDestroy()
        {
            _mainRankingTween?.Kill();
            _subRankingTween?.Kill();
        }

        public async UniTask ClearAllTextAsync()
        {
            var clearTasks = new List<UniTask> { ClearTextAsync(_mainText), ClearTextAsync(_subText) };
            await UniTask.WhenAll(clearTasks);
        }

        public void UpdateHandRankingText(HandRanking handRanking)
        {
            if (handRanking == _lastRanking) return;
            _lastRanking = handRanking;

            if (_subText && _subText.gameObject)
            {
                _subText.ClearText();
                Destroy(_subText.transform.parent.gameObject);
            }
            
            _subText = GetSub();
            if (handRanking is HandRanking.None)
            {
                ClearTextAsync(_mainText).Forget();
                ClearTextAsync(_subText).Forget();
                // TODO: 풀링
                if (_subText.gameObject)
                    Destroy(_subText.transform.parent.gameObject);
                return;
            }
            
            // 트윈 정리 및 Transform 초기화
            var subRect = _subText.transform.parent.GetComponent<RectTransform>();
            _mainRankingTween?.Kill();
            _subRankingTween?.Kill();
            rect.localScale = Vector3.one;
            subRect.anchoredPosition = new Vector2(setting.subPosX, 0);
            
            // DB 접근
            var handRankingData = CardevilCore.Instance.Database.Database.HandRankingDataList
                .FirstOrDefault(d => d.Ranking == handRanking);
            Debug.Assert(handRankingData != null, $"Can't find HandRanking Data: {handRanking}");
            
            // 트윈
            _mainRankingTween = rect
                .DOScale(setting.mainRankingScaleValue, setting.mainRankingChangeDur)
                .SetLoops(2, LoopType.Yoyo)
                .SetAutoKill(true)
                .SetLink(rect.gameObject);

            _subRankingTween = subRect
                .DOAnchorPosX(0f, setting.subRankingChangeDur)
                .SetAutoKill(true)
                .SetLink(_subText.gameObject);
            
            _mainText.UpdateText(handRankingData.DisplayName);
            _subText.UpdateText("+" + handRankingData.Value);
        }

        public async UniTask DoStep(float damage, EvaluationType type)
        {
            var totalDamage = _cachedDamage + damage; 
            
            // 1. Sub Text 이동
            var sub = GetSub();
            var operatorSymbol = type == EvaluationType.Plus ? "+" : "x";
            sub.UpdateText($"{operatorSymbol}{damage}");
            
            var subRect = sub.transform.parent.GetComponent<RectTransform>();
            subRect.anchoredPosition = new Vector2(setting.subPosX, 0);

            var moveTween = subRect
                .DOAnchorPosX(0f, setting.subEvaDur)
                .SetEase(setting.subEvaEase);
            var fadeTween = DOTween.To(
                getter: () => 1f,
                setter: sub.SetAlpha,
                endValue: 0f,
                duration: setting.subEvaDur
            ).SetEase(setting.subFadeOutEase);

            await DOTween.Sequence()
                .Join(moveTween)
                .Join(fadeTween)
                .SetAutoKill()
                .SetRecyclable()
                .SetLink(gameObject);

            // 2. Main Text에 Damage 반영
            float dur = ((damage - _cachedDamage) / 10f + 1) * setting.mainEvaChangeDur; // 데미지에 따라 시간 늘어남.
            
            var scaleTween = rect
                .DOScale(setting.mainRankingScaleValue, setting.mainEvaDur)
                .SetLoops(2, LoopType.Yoyo);
            var numberTween = DOTween.To(
                getter: () => _cachedDamage,
                setter: value => _mainText.UpdateText(value.ToString("0")),
                endValue: totalDamage,
                dur
            );

            await DOTween.Sequence()
                .Join(scaleTween)
                .Join(numberTween)
                .SetAutoKill()
                .SetRecyclable()
                .SetLink(gameObject);

            _cachedDamage = totalDamage;
            sub.ClearText();
            // TODO: 삭제
        }

        public enum EvaluationType
        {
            Plus,
            Multiply
        }

        private TextAnimator GetSub()
        {
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
            Tween fadeOut = DOTween.To(
                getter: () => 1f,
                setter: text.SetAlpha,
                endValue: 0f,
                duration: setting.clearTextDur
            );

            await fadeOut;
            text.ClearText();
        }
    }
}

#if false
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
#endif