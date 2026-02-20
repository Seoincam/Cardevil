using System.Collections.Generic;
using UnityEngine;

namespace Cardevil.NewCard.InStage
{
    public enum OffsetKey
    {
        HandSlot,
        Selection,
        HandCurve,
    }
    
    /// <summary>
    /// 로컬 오프셋들을 합산해, 손패 내에서 최종 목표 Local Position을 계산.
    /// </summary>
    public class LocalPositionResolver
    {
        private readonly Dictionary<OffsetKey, Vector3> _offsets = new();
        
        public void SetOffset(OffsetKey key, Vector3 value)
        {
            if (_offsets.ContainsKey(key))
            {
                ClearOffset(key);
            }
            
            _offsets.Add(key, value);
        }

        public void ClearOffset(OffsetKey key)
        {
            _offsets.Remove(key);
        }

        public Vector3 Resolve()
        {
            var result = Vector3.zero;
            foreach (var offset in _offsets.Values)
            {
                result += offset;
            }

            return result;
        }
    }
}