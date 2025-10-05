namespace Cardevil.DebugConsole.Commands
{
    /// <summary>
    /// 리플렉션으로 등록되는 콘솔 명령어입니다.
    /// </summary>
    public class ReflectionConsoleCommand : IConsoleCommand
    {
        public string Command { get; }
        public string Description { get; }
        private readonly System.Reflection.MethodInfo _method;

        public ReflectionConsoleCommand(string command, string description, System.Reflection.MethodInfo method)
        {
            Command = command;
            Description = description;
            _method = method;
        }

        public void Execute(string[] args)
        {
            _method.Invoke(null, new object[] { args });
        }
    }
}