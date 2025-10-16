using Cardevil.Core;
using Cardevil.Utils;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace Cardevil.Cards.Evaluations
{
    public class AsyncEvaluationEvent : IClearable
    {
        private EvaluationArgsBuilder _builder;
        private SortedList<int, EvaluationArg> _args = new();

        public event Action<EvaluationStep> OnStep;

        public void AddArg(EvaluationArg arg, int priority)
        {
            _args.Add(priority, arg);
        }
        
        public void Clear()
        {
            foreach (var action in _args.Values) action.Release();
            _args.Clear();
        }

        public async UniTask InvokeAsync()
        {
            var damage = 0f;
            // TODO: Text 초기화
            await UniTask.Delay(TimeSpan.FromSeconds(.75f));

            try
            {
                foreach (var action in _args.Values)
                {
                    var before = damage;
                    var after = action.Evaluate(damage, out var effect, out var value);
                    damage = after;

                    OnStep?.Invoke(new EvaluationStep(effect, value, before, after));

                    await UniTask.Delay(TimeSpan.FromSeconds(.7f));
                }

                _builder.SetDamage((int)damage);
            }
            catch (Exception ex)
            {
                LogEx.LogError($"Evaluation Error: {ex}");
            }
            finally
            {
                Clear();
            }            
        }
    }
}
