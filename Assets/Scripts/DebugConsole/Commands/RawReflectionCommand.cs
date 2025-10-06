namespace Cardevil.DebugConsole.Commands
{
    /// <summary>
    /// string[] args를 그대로 사용하는 리플렉션 콘솔 명령어입니다.
    /// </summary>
    public class RawReflectionCommand : IConsoleCommand
    {
        public string Command { get; }
        public string Description { get; }
        private string _signature;
        public string Signature => _signature;
        private readonly System.Reflection.MethodInfo _method;
        
        public RawReflectionCommand(string commandName, string description, System.Reflection.MethodInfo method, string signature = null)
        {
            Command = commandName;
            Description = description;
            _method = method;
            _signature = signature ?? $"{commandName} <string[] args>";
        }
        
        public void Execute(string[] args)
        {
            _method.Invoke(null, new object[] { args });
        }
    }
}