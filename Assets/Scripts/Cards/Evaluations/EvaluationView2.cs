using Cardevil.Cards.Data;
using Cardevil.Cards.ScriptableObjects;
using Cardevil.Core.Bootstrap;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Linq;
using UnityEngine;

namespace Cardevil.Cards.Evaluations
{
    public class EvaluationView2 : MonoBehaviour
    {
        [SerializeField] private CardEvaluationAnimSO setting;
        [SerializeField] private RectTransform rect;
        
        public static EvaluationView2 Current { get; private set; }

        private HandRanking _lastRanking;

        private TextAnimator _mainText;

        private void Awake()
        {
            _mainText = rect.GetComponentInChildren<TextAnimator>();
            
            if (Current && Current != this)
                Destroy(Current);
            Current = this;
        }

        public async UniTask ClearAllTextAsync()
        {
            // var clearTasks = new List<UniTask> { ClearTextAsync(_mainText) };
            // clearTasks.AddRange();
        }

        public void UpdateHandRankingText(HandRanking handRanking)
        {
            if (handRanking == _lastRanking) return;
            _lastRanking = handRanking;
            
            // var sub = GetSub();
            if (handRanking is HandRanking.None)
            {
                ClearTextAsync(_mainText).Forget();
                // ClearTextAsync(sub).Forget();
                // SubPool.Enqueue(sub);
                return;
            }

            var handRankingData = CardevilCore.Instance.Database.Database.HandRankingDataList
                .FirstOrDefault(d => d.Ranking == handRanking);
            Debug.Assert(handRankingData != null, $"Can't find HandRanking Data: {handRanking}");
            
            // TODO: 로직 추가
        }

        public async UniTask DoStep(float damage, EvaluationType type)
        {
            // TODO: 로직 추가
        }

        public enum EvaluationType
        {
            Plus,
            Multiply
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