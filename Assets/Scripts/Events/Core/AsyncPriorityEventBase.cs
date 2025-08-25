using Cardevil.Core;
using System.Collections.Generic;

namespace Cardevil.Events
{
    public abstract class AsyncPriorityEventBase : IClearable
    {
        protected const int MAGIC_NUMBER = -321322;
        
        protected bool _isInvoking = false;
        protected List<int> _keysToClear = new List<int>();
        protected bool _clearAll = false;
        public abstract void Clear();
    }
    
}