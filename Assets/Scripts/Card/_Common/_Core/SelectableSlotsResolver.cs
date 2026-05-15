using Cardevil.Core.Utils;
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
            Direction? defaultDirection,
            IReadOnlyList<CardStateBuilder.SelectableSlot<Direction>> slots)
        {
            var results = new List<Direction>();

            // 네 개인 경우는 UpgradeNode에 의해서 모든 값이 확정되어 있음.
            if (slots.Count == 4)
            {
                foreach (var slot in slots)
                {
                    results.Add(slot.FixedValue);
                }
            }
            
            // 두 개인 경우는 하나를 직접 확정해줘야함.
            else if (slots.Count == 1)
            {
                if (!defaultDirection.HasValue)
                {
                    LogEx.LogError("두 방향이 있는 경우지만, 기본 방향이 존재하지 않습니다.");
                    return null;
                }
                
                results.Add(defaultDirection.Value);
                results.Add(GetOpposite(defaultDirection.Value));
            }
            
            return results;
            
            Direction GetOpposite(Direction dir)
            {
                if (dir == Direction.None) return Direction.None;
                // Up(0), Right(1), Down(2), Left(3)
                return (Direction)(((int)dir + 2) % 4);
            }
        }
    }
}