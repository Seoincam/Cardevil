using System;
using UnityEngine.Scripting;

namespace Cardevil.Test.DebugConsole
{
    /// <summary>
    /// 메서드에 적용하여 콘솔 명령어로 등록합니다.
    /// </summary>
    [Preserve]
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public class ConsoleCommandAttribute : Attribute
    {
        public string Command { get; }
        public string Description { get; }
        
        public string Signature { get;}

        public string[] Arg0AutoComplete { get; }

        public ConsoleCommandAttribute(string command, string description = "", string signature = null, string[] arg0AutoComplete = null)
        {
            Command = command;
            Description = description;
            Signature = signature;
            Arg0AutoComplete = arg0AutoComplete;
        }
    }
    
    [Preserve]
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class ConsoleCommandClassAttribute : Attribute
    {
        public ConsoleCommandClassAttribute()
        {
        }
    }
}