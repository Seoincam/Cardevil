using System;
using System.Collections.Generic;

namespace Cardevil.DebugConsole.Commands
{
    /// <summary>
    /// 콘솔 명령어 인터페이스입니다.
    /// </summary>
    public interface IConsoleCommand
    {
        /// <summary>
        /// 명령어 이름입니다. 실행 시 사용됩니다.
        /// </summary>
        string Command { get; }
        /// <summary>
        /// 명령어 설명입니다. 실행과는 관련 없습니다.
        /// </summary>
        string Description { get; }

        /// <summary>
        /// 명령어 시그니처입니다. (예: "spawn [entityName]")
        /// &lt;&gt; 표시는 필수 인자를 의미합니다.
        /// [] 표시는 선택적 인자를 의미합니다.
        /// </summary>
        string Signature { get; }
        void Execute(string[] args);
        
        void AutoComplete(Span<string> args, ref List<string> suggestions) { }
    }
}