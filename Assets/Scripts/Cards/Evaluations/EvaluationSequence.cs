using Cardevil.Core;
using System.Collections.Generic;
using System.Linq;

namespace Cardevil.Cards.Evaluations
{
    public class EvaluationSequence : IClearable
    {
        private static EvaluationSequence _seq;

        private readonly List<EvaluationStep> _steps = new();
        private int _lastIndex;
        private int _priority = 0;

        private Dictionary<int, List<EvaluationStep>> _groupedSteps;
        
        public static EvaluationSequence Get()
        {
            _seq ??= new EvaluationSequence();
            _seq.Clear();
            return _seq;
        }
        
        public EvaluationSequence Append(EvaluationStep s)
        {
            _lastIndex++;
            _priority = 0;
            
            Insert(s, _lastIndex);
            return this;
        }

        public EvaluationSequence Join(EvaluationStep s)
        {
            _priority++;
            
            Insert(s, _lastIndex);
            return this;
        }

        public void Build()
        {
            _groupedSteps = _steps
                .GroupBy(s => s.IndexOnSequence)
                .OrderBy(g => g.Key)
                .ToDictionary(g => g.Key, 
                    g => g.OrderBy(s => s.Priority).ToList());
        }

        public bool TryGetStepGroup(int index, out List<EvaluationStep> steps)
        {
            if (index < 0 || index > _lastIndex || !_groupedSteps.TryGetValue(index, out steps))
            {
                steps = new();
                return false;
            }

            return true;
        }

        private void Insert(EvaluationStep s, int atIndex)
        {
            // TODO: Index Validate
            
            s.SetIndex(atIndex, _priority);
            _steps.Add(s);
        }
        
        public void Clear()
        {
            _steps.Clear();
            _groupedSteps.Clear();
        }
    }
}