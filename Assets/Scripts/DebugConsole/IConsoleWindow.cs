using UnityEngine;

namespace Cardevil.DebugConsole
{
    public interface IConsoleWindow
    {
        void Open();
        void Close();
        /// <summary>
        /// 콘솔에 메시지를 보냅니다.
        /// </summary>
        void Message(string message);
        void Message(MessageType type, string message);
        void Print(string message);
        void Print(LogType type, string message);
        void ClearHistory();
        void Toggle();
        bool IsOpen { get; }
    }
}