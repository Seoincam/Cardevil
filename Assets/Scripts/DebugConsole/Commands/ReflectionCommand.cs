using Cardevil.Utils;
using System;

namespace Cardevil.DebugConsole.Commands
{
    /// <summary>
    /// 리플렉션으로 등록되는 콘솔 명령어입니다.
    /// </summary>
    public class ReflectionCommand : IConsoleCommand
    {
        public string Command { get; }
        public string Description { get; }
        private readonly System.Reflection.MethodInfo _method;
        private readonly Type[] _paramType;

        private ReflectionCommand(string commandName, string description, System.Reflection.MethodInfo method)
        {
            Command = commandName;
            Description = description;
            _method = method;
            _paramType = Array.ConvertAll(_method.GetParameters(), p => p.ParameterType);
        }

        public void Execute(string[] args)
        {
            try
            {
                var convertedArgs = ConvertArguments(args);
                _method.Invoke(null, convertedArgs);
            }
            catch (Exception ex)
            {
                Console.MessageError($"Error executing command '{Command}': {ex.Message}");
            }
        }
        public static bool Create(string commandName, string description, System.Reflection.MethodInfo method, out ReflectionCommand command)
        {
            if (!method.IsStatic)
            {
                command = null;
                return false;
            }

            command = new ReflectionCommand(commandName, description, method);
            if (!AreAllParametersSupported(command._paramType))
            {
                command = null;
                return false;
            }
            return true;
        }
        
        private static bool IsSupportedType(Type type)
        {
            return type == typeof(int) || type == typeof(float) || type == typeof(bool) || type == typeof(string) || type.IsEnum;
        }
        
        private static bool AreAllParametersSupported(Type[] types)
        {
            foreach (var type in types)
            {
                if (!IsSupportedType(type))
                    return false;
            }
            return true;
        }
        
        private object[] ConvertArguments(string[] args)
        {
            if (args.Length != _paramType.Length)
                throw new ArgumentException($"Argument count mismatch. Expected {_paramType.Length}, got {args.Length}");

            object[] convertedArgs = new object[args.Length];
            for (int i = 0; i < args.Length; i++)
            {
                var targetType = _paramType[i];
                var arg = args[i];
                if (targetType == typeof(int))
                    convertedArgs[i] = this.ParseArgument<int>(arg);
                else if (targetType == typeof(float))
                    convertedArgs[i] = this.ParseArgument<float>(arg);
                else if (targetType == typeof(bool))
                    convertedArgs[i] = this.ParseArgument<bool>(arg);
                else if (targetType == typeof(string))
                    convertedArgs[i] = this.ParseArgument<string>(arg);
                else if (targetType.IsEnum)
                {
                    if (Enum.TryParse(targetType, arg, true, out object enumValue))
                        convertedArgs[i] = enumValue;
                    else if(int.TryParse(arg, out int enumInt))
                        convertedArgs[i] = Enum.ToObject(targetType, enumInt);
                    else
                        throw new ArgumentException($"Cannot parse argument '{arg}' to enum type {targetType}");
                }
                else
                {
                    throw new ArgumentException($"Unsupported parameter type: {targetType}");
                }
            }
            return convertedArgs;
        }
        

        
    }
}