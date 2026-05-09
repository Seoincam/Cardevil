using System;

namespace Cardevil.Core.Utils
{
    /// <summary>
    /// Unity에서 직렬화할 수 없는 <see cref="Nullable{T}"/> (<c>T?</c>)를 대체하기 위한 구조체.  
    /// 값의 존재 여부(<see cref="HasValue"/>)와 실제 값(<see cref="Value"/>)을 함께 저장.
    /// </summary>
    /// <typeparam name="T">값의 타입. <see cref="struct"/> 제약이 있어 <see cref="int"/>, <see cref="Enum"/> 등을 사용할 수 있음.</typeparam>
    [Serializable]
    public struct Optional<T> where T : struct
    {
        /// <summary>
        /// 값이 존재하는지를 나타내는 플래그.  
        /// <c>true</c>면 <see cref="Value"/>가 유효한 값을 가지고 있음.
        /// </summary>
        public bool HasValue { get; private set; }
        
        /// <summary>
        /// 실제 저장된 값.  
        /// <see cref="HasValue"/>가 <c>false</c>인 경우, 기본값(<c>default</c>)을 가짐.
        /// </summary>
        public T Value { get; private set; }
        
        /// <param name="value">
        /// 초기값으로 사용할 <see cref="Nullable{T}"/>.  
        /// <c>null</c>이면 <see cref="HasValue"/>는 <c>false</c>로 설정.
        /// </param>
        public Optional(T? value)
        {
            if (value.HasValue)
            {
                HasValue = true;
                this.Value = value.Value;
            }
            else
            {
                HasValue = false;
                this.Value = default;
            }
        }
        
        public static implicit operator T(Optional<T> optional) => optional.Value;
    }
}