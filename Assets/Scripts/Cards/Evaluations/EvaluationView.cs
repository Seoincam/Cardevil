using Cardevil.Cards.Data;
using Cardevil.Cards.ScriptableObjects;
using Cardevil.Core;
using Cardevil.Utils;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using NotImplementedException = System.NotImplementedException;

namespace Cardevil.Cards.Evaluations
{
    public class EvaluationView : MonoBehaviour, IClearable
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
        
        public void UpdateHandRankingText(HandRanking ranking)
        {
            if (ranking == _lastRanking) return;
            _lastRanking = ranking;

            var sub = GetSub();
            var subRect= sub.transform.parent.GetComponent<RectTransform>();

            if (ranking is HandRanking.None or HandRanking.High)
            {
                _mainText.ClearText();
                sub.ClearText();
                SubPool.Enqueue(sub);
                return;
            }

            var data = Managers.Database.Database.HandRankingDataList
                .FirstOrDefault(i => i.Ranking == ranking);
            if (data == null) { LogEx.LogError($"Can't find HandRanking Data: {ranking}"); return; }

            // 이전 Tween 정리 및 Transform 초기화
            _mainRankingTween?.Kill();
            _subRankingTween?.Kill();
            main.localScale = Vector3.one;
            subRect.anchoredPosition = new Vector3(animSO.s_posX, 0);

            // 새 Tween
            _mainRankingTween = main.DOScale(1.2f, animSO.m_RankingChangeDur)
                .SetLoops(2, LoopType.Yoyo)
                .SetAutoKill(true).SetLink(main.gameObject);
            _subRankingTween = subRect.DOAnchorPosX(0, animSO.s_RankingChangeDur)
                .SetAutoKill(true).SetLink(sub.gameObject);

            _mainText.UpdateText(data.DisplayName);
            sub.UpdateText("+" + data.Value.ToString());
            
            SubPool.Enqueue(sub);
        }

        // 스텝을 등록함
        public void RegisterStep(EvaluationStep s)
        {
            if (s.Type is EvaluationStepType.None or EvaluationStepType.Move) 
                return;
            
            var sub = GetSub();
            var rect = sub.transform.parent.GetComponent<RectTransform>();
            
            _stepSeq?.Kill();
            _stepSeq = DOTween.Sequence().SetAutoKill(true).SetLink(sub.gameObject);
            
            rect.anchoredPosition= new Vector3(animSO.s_posX * 1.5f, _lastStepY);
            string oper = s.Type == EvaluationStepType.Plus ? "+" : "x"; // TODO: 더 제대로 나눠야함.
            sub.UpdateText($"{oper}{s.Value}");
            
            // 이 부분을 수정해야함. 
            // 중간에 멈춰있도록 수정하기.
            _stepSeq
                .Append(rect.DOAnchorPosX(animSO.s_posX, animSO.s_evaDur).SetEase(animSO.s_evaEase));
            
            RegisteredSubs.Enqueue(sub);
            _lastStepY += 100f;
        }
        
        private float _prevDamage; // 데미지 캐시 
        
        // 등록된 스텝을 일괄 실행
        public async UniTask DoStep(float totalDamage)
        {
            _mainText.UpdateText(_prevDamage.ToString());

            var subs = new List<TextAnimator>(RegisteredSubs.Count);
            while(RegisteredSubs.Count > 0)
                subs.Add(RegisteredSubs.Dequeue());

            var waitAll = new List<UniTask>(subs.Count);

            foreach (var sub in subs)
            {
                var rect = sub.transform.parent.GetComponent<RectTransform>();

                var seq = DOTween.Sequence()
                    .Append(rect.DOAnchorPosX(0, animSO.s_evaDur).SetEase(animSO.s_evaEase))
                    .SetAutoKill()
                    .SetRecyclable()
                    .SetLink(gameObject);

                waitAll.Add(seq.AwaitForComplete());
            }
            
            await UniTask.WhenAll(waitAll);
            
            var mainSeq = DOTween.Sequence()
                .Append(main.DOScale(1.1f, animSO.m_evaDur))
                .Join(DOTween.To(
                    () => _prevDamage,
                    v => _mainText.UpdateText(v.ToString("0")),
                    totalDamage,
                    animSO.m_evaChangeDur))
                .SetAutoKill()
                .SetRecyclable()
                .SetLink(gameObject);
           await mainSeq;
           
            _prevDamage = totalDamage;
            _lastStepY = 0f;

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
            var go = Managers.Resource.Instantiate(path, transform);
            if (!go)
            {
                LogEx.LogError($"Can't find Evaluation View Sub: {path}");
                return null;
            }
            
            var sub = go.GetComponentInChildren<TextAnimator>();
            return sub;
        }

        public void Clear()
        {
            _prevDamage = 0f;
        }
    }
}
