using Cardevil.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cardevil.Card.Common.Core
{
    /// <summary>
    /// 전투 시작 시 미정 값들을 실제 값으로 해소해주는 유틸.
    /// </summary>
    public static class SelectableSlotsResolver
    {
        public static IReadOnlyList<CardColor> ResolveAlternativeColors(
            Optional<CardColor> defaultColor,
            IReadOnlyList<Optional<CardColor>> alternatives)
        {
            if (!defaultColor.HasValue)
            {
                throw new ArgumentNullException(nameof(defaultColor));
            }   
            
            var used = new HashSet<CardColor> { defaultColor.Value };
            var resolvedAlternatives = new List<CardColor>(alternatives.Count);

            foreach (var alternative in alternatives)
            {
                if (alternative.HasValue)
                {
                    used.Add(alternative.Value);
                    resolvedAlternatives.Add(alternative.Value);
                }
            }
            
            var available = new[] { CardColor.Red, CardColor.Green, CardColor.Blue, CardColor.Black }
                .Where(n => !used.Contains(n))
                .ToList();

            foreach (var alternative in alternatives)
            {
                if (alternative.HasValue) continue;
                
                int index = RandomUtil.GetRandomInt(0, available.Count);
                var picked = available[index];
                resolvedAlternatives.Add(picked);
                available.RemoveAt(index);
            }

            return resolvedAlternatives;
        }

        public static IReadOnlyList<int> ResolveAlternativeNumbers(
            Optional<int> defaultNumber,
            IReadOnlyList<Optional<int>> alternatives)
        {
            var used = new HashSet<int>();
            var resolvedAlternatives = new List<int>(alternatives.Count);

            if (defaultNumber.HasValue)
            {
                used.Add(defaultNumber.Value);
            }

            foreach (var alternative in alternatives)
            {
                if (alternative.HasValue)
                {
                    used.Add(alternative.Value);
                    resolvedAlternatives.Add(alternative.Value);
                }
            }

            var available = Enumerable.Range(2, 9)
                .Where(n => !used.Contains(n))
                .ToList();

            foreach (var alternative in alternatives)
            {
                if (alternative.HasValue) continue;
                
                int index = RandomUtil.GetRandomInt(0, available.Count);
                var picked = available[index];
                resolvedAlternatives.Add(picked);
                available.RemoveAt(index);
            }

            return resolvedAlternatives;
        }

        public static IReadOnlyList<Direction> ResolveDirections(
            Direction? defaultDirection,
            IReadOnlyList<Direction?> alternatives)
        {
            Direction GetOpposite(Direction dir)
            {
                if (dir == Direction.None) return Direction.None;
                // Up(0), Right(1), Down(2), Left(3)
                return (Direction)(((int)dir + 2) % 4);
            }
            
            
            var resolved = new List<Direction>();

            // 네 개인 경우는 UpgradeNode에 의해 값이 이미 정해져있음.
            if (alternatives.Count == 4)
            {
                foreach (var alternative in alternatives)
                {
                    resolved.Add(alternative.Value);
                }
            }
            
            else if (alternatives.Count == 1)
            {
                if (!defaultDirection.HasValue)
                {
                    LogEx.LogError("해소 중 두 방향이 존재하지만 기본 방향이 존재하지 않는 카드를 확인했습니다.");
                    return null;
                }
                
                resolved.Add(defaultDirection.Value);
                resolved.Add(GetOpposite(defaultDirection.Value));
            }

            return resolved;
        }
    }
}