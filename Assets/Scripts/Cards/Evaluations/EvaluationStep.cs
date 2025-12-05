using Cardevil.Attributes;
using Cardevil.Core;
using Cardevil.Relics;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace Cardevil.Cards.Evaluations
{
    [Serializable]
    public sealed class EvaluationStep : IClearable
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
            Value = 0;
            _visuals.Clear();
        }
        
        public Type StepType { get; private set; }
        public float Value { get; private set; }
        private List<IEvaluateVisual> _visuals = new();

        public int IndexOnSequence { get; private set; }
        public int Priority { get; private set; }

        public void SetIndex(int index, int priority)
        {
            IndexOnSequence = index;
            Priority = priority;
        }
        
        public EvaluationStep SetValue(Type type, float value = 0)
        {
            StepType = type;
            Value = value;
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

        public void CalculateDamage(ref float totalDamage)
        {
            if (StepType is Type.None or Type.Move)
                return;

            switch (StepType)
            {
                case Type.Plus:
                    totalDamage += Value;
                    break;
                case Type.Multiply:
                    totalDamage *= Value;
                    break;
            }
        }
        
        public enum Type
        {
            None,
            Move, 
            Plus, Multiply
        }

        public void ExecuteVisualEffect()
        {
            foreach (var visual in _visuals)
                visual.ExecuteEvaluationAction();
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

