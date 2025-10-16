using Cardevil.Utils;
using Cardevil.Utils.Directions;
using System.Collections.Generic;
using System.Linq;

namespace Cardevil.Cards.Data.InStage
{
    /// <summary>
    /// 선택 가능한 값들의 상태를 관리하는 제네릭 클래스.  
    /// 숫자(<see cref="int"/>)나 방향(<see cref="Direction"/>) 선택형 데이터에 사용.
    /// </summary>
    /// <typeparam name="T">값의 타입 (<see cref="int"/>, <see cref="Direction"/>)</typeparam>

    public class SelectState<T> where T : struct
    {
        /// <summary>
        /// 선택 가능한 값들의 읽기 전용 목록.  
        /// 확정되지 않은 값은 <c>null</c>로 유지.
        /// </summary>
        public IReadOnlyList<T?> Selectables => _selectables;
        
        /// <summary>
        /// 최종 확정된 값.  
        /// 선택지가 하나뿐이라면 그 값을 반환,  
        /// 아직 확정이 되지 않은 경우 <c>null</c>를 반환.
        /// </summary>
        public T? FinalValue => 
            _selectables.Count == 1 && _selectables[0].HasValue
                ? _selectables[0].Value
                : _selectedValue;
        
        private readonly List<T?> _selectables;
        private T? _selectedValue;
        
        /// <summary>
        /// 첫 번째 null(미확정) <see cref="T"/>를 <paramref name="value"/>로 확정.
        /// </summary>
        public bool TrySetFirstNull(T value)
        {
            for (int i = 0; i < _selectables.Count; i++)
            {
                if (!_selectables[i].HasValue)
                {
                    _selectables[i] = value;
                    return true;
                }
            }
            return false;
        }
        
        public SelectState(List<T?> selectables)
        {
            _selectables = selectables;
        }
    }

    public static class NumberSelectStateExtensions
    {
        /// <summary>
        /// 숫자 선택 확정: null 슬롯을 2~10 중 중복 없는 값으로 채움.
        /// </summary>
        public static void ConfirmNullValues(this SelectState<int> state)
        {
            var availableNumbers = Enumerable.Range(2, 9).ToList();
            
            // 이미 확정된 숫자 제거
            foreach (var v in state.Selectables)
            {
                if (v.HasValue)
                    availableNumbers.Remove(v.Value);
            }
            
            // 남은 미확정 숫자 null 랜덤으로 채우기
            while (availableNumbers.Count > 0)
            {
                int randomIndex = UnityEngine.Random.Range(0, availableNumbers.Count);
                int v = availableNumbers[randomIndex];
                if (!state.TrySetFirstNull(v)) break;
                availableNumbers.RemoveAt(randomIndex);
            }
        }
    }
    
    public static class MoveSelectStateExtensions
    {
        /// <summary>
        /// 방향 선택 확정:
        /// - 슬롯 2개이고 첫 슬롯만 확정되어 있으면 두 번째는 반대방향으로.
        /// - 그 외엔 중복되지 않는 임의의 방향으로 채움.
        /// </summary>
        public static void ConfirmNullValues(this SelectState<Direction> state)
        {
            // 2칸 구조에서 [A, null]이면, 두 번째를 A의 반대로
            if (state.Selectables.Count == 2 && state.Selectables[0].HasValue && !state.Selectables[1].HasValue)
            {
                state.TrySetFirstNull(state.Selectables[0].Value.Opposite());
                return;
            }
            
            if (state.Selectables.Count == 4)
            {
                var availableDirections =
                    new List<Direction>() { Direction.Up, Direction.Down, Direction.Left, Direction.Right };

                foreach (var d in availableDirections)
                {
                    if (!state.Selectables.Contains(d))
                        state.TrySetFirstNull(d);
                }

                return;
            }
            
            LogEx.LogError("방향 확정 중 의도치 않은 경우가 있음!");
        }
    }
}