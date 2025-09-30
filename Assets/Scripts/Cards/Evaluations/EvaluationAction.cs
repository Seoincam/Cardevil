using Cardevil.Core;
using Cardevil.Relics;
using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

namespace Cardevil.Cards.Evaluations
{    
    /// <summary>
    /// 카드를 사용했을 때 반응해야 할 개체가 구현. 
    /// </summary>
    public interface IEvaluateVisual
    {
        /// <summary>
        /// Evaluation 시 반응.
        /// </summary>
        public void ExecuteEvaluationAction();
    }

    public class EvaluationAction : IClearable, IDisposable
    {
        #region Pooling

        private static readonly Queue<EvaluationAction> pool = new();

        public static EvaluationAction Get()
        {
            var action = pool.Count > 0 ? pool.Dequeue() : new();
            return action;
        }

        public void Release()
        {
            Clear();
            pool.Enqueue(this);
        }

        /// <summary>
        /// 주의: 이 Dispose는 자원 정리 용도가 아님.
        /// using 블록이 끝날 때 자동으로 이벤트 매니저에 등록.
        /// </summary>
        public void Dispose()
        {
            Managers.Card.Evaluations.AddAction(this, _priority);
        }

        public void Clear()
        {
            _effectType = EvaluationEffect.None;
            _value = 0;
            _visuals.Clear();
        }

        #endregion
        
        public enum EvaluationEffect
        {
            None, Move, Plus, Multiply
        }

        private EvaluationEffect _effectType;
        private float _value;
        private List<IEvaluateVisual> _visuals = new();
        private int _priority;

        public void SetValue(int priority, EffectEvaluation damageType, float value = 0)
        {
            _priority = priority;

            _effectType = damageType switch
            {
                EffectEvaluation.MultiplyRanking => EvaluationEffect.Multiply,
                EffectEvaluation.MultiplyAll => EvaluationEffect.Multiply,
                EffectEvaluation.Plus => EvaluationEffect.Plus,
                _ => EvaluationEffect.Move
            };

            _value = value;
        }

        public void SetVisual<T>(T visual) where T : IEvaluateVisual
        {
            _visuals.Add(visual);
        }

        public void SetVisual<T>(List<T> visuals) where T : IEvaluateVisual
        {
            _visuals.AddRange(visuals.Cast<IEvaluateVisual>());
        }

        public float Evaluate(Text text, float damage)
        {
            switch (_effectType)
            {
                case EvaluationEffect.Plus: damage += _value; break;
                case EvaluationEffect.Multiply: damage *= _value; break;
                default: break;
            }

            foreach (var visual in _visuals)
                visual.ExecuteEvaluationAction();

            // 임시
            var seq = DOTween.Sequence();
            seq.Append(text.transform.DOScale(1.2f, 0.15f)) // 커짐
            .AppendCallback(() =>
            {
                text.text = damage.ToString();
            })
            .Append(text.transform.DOScale(1f, 0.15f)); // 원래 크기로 복귀

            return damage;
        }
    }
}

