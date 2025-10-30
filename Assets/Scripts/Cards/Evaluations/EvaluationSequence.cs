using Cardevil.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cardevil.Cards.Evaluations
{
    public class EvaluationSequence : IClearable
    {
        private static EvaluationSequence _instance;

        private readonly List<EvaluationStep> _steps = new();
        private int _lastIndex;
        private int _priority;

        private Dictionary<int, List<EvaluationStep>> _groupedSteps;
        
        public static EvaluationSequence Get()
        {
            _instance ??= new EvaluationSequence();
            _instance.Clear();
            return _instance;
        }
        
        public EvaluationSequence Append(EvaluationStep s)
        {
            _priority = 0;
            Insert(s, _lastIndex);
            return this;
        }

        public EvaluationSequence Join(EvaluationStep s)
        {
            if (_lastIndex <= 0)
                throw new InvalidOperationException("Join requires at least one prior Append!");
            
            _lastIndex--;
            _priority++;
            Insert(s, _lastIndex);
            
            return this;
        }

        public void Build()
        {
            _groupedSteps = _steps
                .GroupBy(s => s.IndexOnSequence)
                .ToDictionary(
                    g => g.Key, 
                    g => g.OrderBy(s => s.Priority).ToList());
        }

        public bool TryGetStepGroup(int index, out List<EvaluationStep> steps)
        {
            if (_groupedSteps == null || index < 0 || index >= _lastIndex || !_groupedSteps.TryGetValue(index, out steps))
            {
                steps = null;
                return false;
            }

            return true;
        }

        private void Insert(EvaluationStep s, int atIndex)
        {
            if (atIndex < 0 || atIndex > _lastIndex)
                throw new ArgumentOutOfRangeException(nameof(atIndex));
            
            s.SetIndex(atIndex, _priority);
            _steps.Add(s);
            
            _lastIndex = Math.Max(_lastIndex, atIndex + 1);

            _groupedSteps = null; // 데이터 변경 시 Build 결과 무효화
        }
        
        public void Clear()
        {
            _steps.Clear();
            _groupedSteps = null;
            _lastIndex = 0;
            _priority = 0;
        }
    }
}