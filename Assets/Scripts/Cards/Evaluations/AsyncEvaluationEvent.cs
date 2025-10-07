using Cardevil.Core;
using Cardevil.Utils;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace Cardevil.Cards.Evaluations
{
    public class AsyncEvaluationEvent : IClearable
    {
        private SortedList<int, EvaluationAction> _actions = new();

        public event Action<EvaluationStep> OnStep;

        public void AddAction(EvaluationAction action, int priority)
        {
            _actions.Add(priority, action);
        }

        public async UniTask InvokeAsync()
        {
            var damage = 0f;
            // TODO: Text 초기화
            await UniTask.Delay(TimeSpan.FromSeconds(.75f));

            try
            {
                foreach (var action in _actions.Values)
                {
                    var before = damage;
                    var after = action.Evaluate(damage, out var effect, out var value);
                    damage = after;

                    OnStep?.Invoke(new EvaluationStep(effect, value, before, after));

                    await UniTask.Delay(TimeSpan.FromSeconds(.7f));
                }

                Managers.Card.ResultCtx.CommitedResult.UpdateDamage(damage);
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

        public void Clear()
        {
            foreach (var action in _actions.Values) action.Release();
            _actions.Clear();
        }
    }
}
