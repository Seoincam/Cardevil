using Cardevil.Core;
using Cardevil.Utils;
using Cardevil.Utils.Directions;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

namespace Cardevil.Cards.Data.InStage
{
    /// <summary>
    /// 선택 가능한 값들의 상태를 관리하는 제네릭 클래스.  
    /// 숫자(<see cref="int"/>)나 방향(<see cref="Direction"/>) 선택형 데이터에 사용.
    /// </summary>
    /// <typeparam name="T">값의 타입 (<see cref="int"/>, <see cref="Direction"/>)</typeparam>\
    [Serializable]
    public class SelectState<T> : IClearable where T : struct
    {
        [SerializeField] private List<Optional<T>> selectables = new();
        [SerializeField] private Optional<T> selectedValue;
        
        private List<Optional<T>> _initialSelectables = new(); // Clear() 시 해당 값으로 초기화
        
        /// <summary>
        /// 선택 가능한 값들의 읽기 전용 목록.  
        /// 확정되지 않은 값은 <c>null</c>로 유지.
        /// </summary>
        public IReadOnlyList<Optional<T>> Selectables => selectables;
        
        /// <summary>
        /// 최종 확정된 값.  
        /// 아직 확정이 되지 않은 경우 <c>null</c>을 반환.
        /// </summary>
        public T? FinalValue
        {
            get
            {
                if (selectables.Count == 1 && selectables[0].hasValue)
                    return selectables[0].value;
                
                if (selectedValue.hasValue)
                    return selectedValue.value;

                return null;
            }
        }
        
        /// <summary>
        /// 첫 번째 null(미확정) <see cref="T"/>를 <paramref name="value"/>로 확정.
        /// </summary>
        public bool TrySetFirstNull(T value)
        {
            for (int i = 0; i < selectables.Count; i++)
            {
                if (selectables[i].hasValue)
                    continue;
                
                selectables[i] = new Optional<T>(value);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 값 선택을 시도.
        /// </summary>
        /// <returns>시도한 값이 선택 가능한 값인지 여부.</returns>
        public bool TrySelect(T value)
        {
            bool contain = false;
            foreach (var selectable in selectables)
            {
                if (!selectable.value.Equals(value))
                    continue;
                contain = true;
                break;
            }

            if (!contain)
                return false;

            selectedValue.value = value;
            selectedValue.hasValue = true;
            return true;
        }
        
        public SelectState(List<T?> selectables)
        {
            foreach (var selectable in selectables)
            {
                this.selectables.Add(new Optional<T>(selectable));
                _initialSelectables.Add(new Optional<T>(selectable));
            }
        }

        public void Clear()
        {
            selectedValue = new Optional<T>(null);
            selectables.Clear();
            foreach (var value in _initialSelectables)
                selectables.Add(value);
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
                if (v.hasValue)
                    availableNumbers.Remove(v.value);
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
}