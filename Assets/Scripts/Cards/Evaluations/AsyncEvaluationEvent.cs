using Cardevil.Core;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Cardevil.Cards.Evaluations
{
    public class AsyncEvaluationEvent : IClearable
    {
        private SortedList<int, EvaluationAction> _actions = new();

        public void AddAction(EvaluationAction action, int priority)
        {
            _actions.Add(priority, action);
        }

        public async UniTask InvokeAsync(Text text)
        {
            var damage = 0f;
            var seq = DOTween.Sequence();
            seq.Append(text.transform.DOScale(1.2f, .15f))
            .AppendCallback(() =>
            {
                text.text = damage.ToString();
            })
            .Append(text.transform.DOScale(1f, .15f));

            await UniTask.Delay(TimeSpan.FromSeconds(.75f));

            try
            {
                foreach (var action in _actions.Values)
                {
                    damage = action.Evaluate(text, damage);
                    await UniTask.Delay(TimeSpan.FromSeconds(1f));
                }

                Managers.Card.ResultCtx.CommitedResult.UpdateDamage(damage);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Evaluation Error: {ex}");
            }
            finally
            {
                Clear();
            }            
        }

        public void Clear()
        {
            foreach (var action in _actions.Values)
                action.Release();
            _actions.Clear();
        }
    }
}
