using Cardevil.Core;
using Cardevil.Utils;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace Cardevil.Cards.Evaluations
{
    public class AsyncEvaluationEvent : IClearable
    {
        private readonly EvaluationArgsBuilder _builder;
        private EvaluationUIAnimator _animator;
        private readonly SortedList<int, EvaluationArg> _args = new();

        public event Action<EvaluationStep> OnStep;
        
        public EvaluationUIAnimator Animator => _animator;

        public void AddArg(EvaluationArg arg, int priority)
        {
            _args.Add(priority, arg);
        }

        public AsyncEvaluationEvent(EvaluationArgsBuilder builder)
        {
            _builder = builder;
            
            // Evaluation UI Animator
            string path = "UI/CardUI/Evaluation Visual";
            var go = Managers.Resource.Instantiate(path).gameObject;
            if (!go)
            {
                LogEx.LogError($"Evaluation UI Animator가 존재하지 않음! path: {path}");
                return;
            }
            _animator = go.GetComponent<EvaluationUIAnimator>();
            _animator.Init(this);
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
