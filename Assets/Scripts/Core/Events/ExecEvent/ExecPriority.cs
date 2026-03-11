using System;

namespace Cardevil.Events.ExecEvents
{
    public enum ExecPriority
    {
        First = Int32.MinValue,
        
        
        Normal = 0,
        
        
        Last = Int32.MaxValue // UI등등
    }
}