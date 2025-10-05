using System;
using UnityEngine.Scripting;

namespace Cardevil.DebugConsole
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

        public ConsoleCommandAttribute(string command, string description = "")
        {
            Command = command;
            Description = description;
        }
    }
}