using Cardevil.DebugConsole.Commands;
using System;

namespace Cardevil.DebugConsole
{
    public static class CommandHelper
    {
        public static T ParseArgument<T>(this IConsoleCommand consoleCommand, string arg)
        {
            try
            {
                return ParseArgument<T>(arg);
            }
            catch (ArgumentException)
            {
                throw new ArgumentException($"Error in command '{consoleCommand.Command}': Cannot parse argument '{arg}' to type {typeof(T)}");
            }
        }
        
        public static T ParseArgument<T>(string arg)
        {
            if (typeof(T) == typeof(int))
            {
                if (int.TryParse(arg, out int result))
                    return (T)(object)result;
            }
            else if (typeof(T) == typeof(float))
            {
                if (float.TryParse(arg, out float result))
                    return (T)(object)result;
            }
            else if (typeof(T) == typeof(bool))
            {
                if (bool.TryParse(arg, out bool result))
                    return (T)(object)result;
                if (arg == "1")
                    return (T)(object)true;
                if (arg == "0")
                    return (T)(object)false;
                if (arg.ToLower().Equals("t", StringComparison.OrdinalIgnoreCase))
                    return (T)(object)true;
                if (arg.ToLower().Equals("f", StringComparison.OrdinalIgnoreCase))
                    return (T)(object)false;
            }
            else if (typeof(T) == typeof(string))
            {
                return (T)(object)arg;
            }
            
            throw new ArgumentException($"Cannot parse argument '{arg}' to type {typeof(T)}");
        }
    }
}