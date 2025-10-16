using System.Collections.Generic;

namespace Cardevil.Cards.Data.InStage
{
    public class SelectState<T> where T : struct
    {
        private readonly List<T?> _selectables;
        private T? _selectedValue;
        
        public T? FinalValue => 
            _selectables.Count == 1 && _selectables[0].HasValue
                ? _selectables[0].Value
                : _selectedValue;
        
        public SelectState(List<T?> selectables)
        {
            _selectables = selectables;
        }
    }
}