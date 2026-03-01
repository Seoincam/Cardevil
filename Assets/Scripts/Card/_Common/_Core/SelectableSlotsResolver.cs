using Cardevil.Utils;
using Cardevil.Utils.Directions;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Cardevil.Card.Common.Core
{
    /// <summary>
    /// 전투 시작 시 미정 슬롯을 실제 값으로 해소해주는 유틸.
    /// Spec -> State 빌드 시 호출됨.
    /// </summary>
    public static class SelectableSlotsResolver
    {
        public static IReadOnlyList<CardColor> ResolveColors(
            CardColor? defaultColor,
            IReadOnlyList<CardStateBuilder.SelectableSlot<CardColor>> slots)
        {
            if (!defaultColor.HasValue)
            {
                throw new ArgumentNullException(nameof(defaultColor));
            }
            
            var used = new HashSet<CardColor> { defaultColor.Value };
            var results = new List<CardColor>(slots.Count);

            foreach (var slot in slots)
            {
                if (slot.IsFixed)
                {
                    used.Add(slot.FixedValue);
                    results.Add(slot.FixedValue);
                }
            }
            
            var available = new[] { CardColor.Red, CardColor.Green, CardColor.Blue, CardColor.Black }
                .Where(n => !used.Contains(n))
                .ToList();
            
            foreach (var slot in slots)
            {
                if (slot.IsFixed) continue;

                int index = RandomUtil.GetRandomInt(0, available.Count);
                var picked = available[index];
                results.Add(picked);
                available.RemoveAt(index);
            }
            
            return results;
        }
        
        public static IReadOnlyList<int> ResolveNumbers(
            int? defaultNumber,
            IReadOnlyList<CardStateBuilder.SelectableSlot<int>> slots)
        {
            var used = new HashSet<int>();
            if (defaultNumber != null)
            {
                used.Add(defaultNumber.Value);
            }
            
            var results = new List<int>(slots.Count);

            foreach (var slot in slots)
            {
                if (slot.IsFixed)
                {
                    used.Add(slot.FixedValue);
                    results.Add(slot.FixedValue);
                }
            }
            
            var available = Enumerable.Range(2, 9)
                .Where(n => !used.Contains(n))
                .ToList();

            foreach (var slot in slots)
            {
                if (slot.IsFixed) continue;

                int index = RandomUtil.GetRandomInt(0, available.Count);
                var picked = available[index];
                results.Add(picked);
                available.RemoveAt(index);
            }
            
            return results;
        }

        public static IReadOnlyList<Direction> ResolveDirections(
            IReadOnlyList<CardStateBuilder.SelectableSlot<Direction>> slots)
        {
            var results = new List<Direction>();
            
            foreach (var slot in slots)
            {
                // Move 카드의 값들은 모두 확정되어 있어야 함.
                if (!slot.IsFixed)
                {
                    Debug.LogError("Move 카드의 값이 확정되지 않았음.");
                    return null;
                }
                
                results.Add(slot.FixedValue);
            }
            
            return results;
        }
    }
}