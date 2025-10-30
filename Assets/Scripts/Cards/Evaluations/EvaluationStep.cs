using Cardevil.Core;
using Cardevil.Relics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cardevil.Cards.Evaluations
{
    public enum EvaluationStepType
    {
        None,
        Move, 
        Plus, Multiply
    }
    
    // 값 신경 쓰지 말고 비주얼만.
    public class EvaluationStep : IClearable
    {
        private static readonly Queue<EvaluationStep> Pool = new();

        public static EvaluationStep Get()
        {
            var action = Pool.Count > 0 ? Pool.Dequeue() : new();
            return action;
        }

        public void Release()
        {
            Clear();
            Pool.Enqueue(this);
        }

        public void Clear()
        {
            _value = 0;
            _visuals.Clear();
        }
        
        private EvaluationStepType _type;
        private float _value;
        private List<IEvaluateVisual> _visuals = new();

        public int IndexOnSequence { get; private set; }
        public int Priority { get; private set; }

        public void SetIndex(int index, int priority)
        {
            IndexOnSequence = index;
            Priority = priority;
        }
        
        public EvaluationStep SetValue(EvaluationStepType type, float value = 0)
        {
            _type = type;
            _value = value;
            return this;
        }

        public EvaluationStep SetVisual<T>(T visual) where T : IEvaluateVisual
        {
            _visuals.Add(visual);
            return this;
        }

        public EvaluationStep SetVisual<T>(List<T> visuals) where T : IEvaluateVisual
        {
            _visuals.AddRange(visuals.Cast<IEvaluateVisual>());
            return this;
        }

        // public float Evaluate(float damage, out EvaluationEffect effect, out float value)
        // {
        //     switch (_effectType)
        //     {
        //         case EvaluationEffect.Plus: damage += _value; break;
        //         case EvaluationEffect.Multiply: damage *= _value; break;
        //         default: break;
        //     }
        //
        //     foreach (var visual in _visuals)
        //         visual.ExecuteEvaluationAction();
        //
        //     effect = _effectType;
        //     value = _value;
        //     return damage;
        // }
    }
}

