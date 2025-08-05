using System.Collections.Generic;
using Unity.VisualScripting;

namespace Cardevil.DataStructure
{
    public class SubscriptableVariable<T>
    {
        private T _value;
        public T Value
        {
            get => _value;
            set
            {
                if (!EqualityComparer<T>.Default.Equals(_value, value))
                {
                    T oldValue = _value;
                    _value = value;
                    OnValueChanged?.Invoke(oldValue, _value);
                }
            }
        }

        public delegate void ValueChangedDelegate(T oldValue, T newValue);
        public event ValueChangedDelegate OnValueChanged;
        public SubscriptableVariable(T initialValue)
        {
            _value = initialValue;
        }
        public SubscriptableVariable() : this(default(T)) { }
    }
}