using Cardevil.Core.Bootstrap;
using Cardevil.DebugConsole;
using Cardevil.DebugConsole.Commands;
using Cardevil.Sound;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;
using Console = Cardevil.DebugConsole.Console;

namespace Cardevil.Sounds
{
    
    /// <summary>
    /// 콘솔 명령어로 사운드를 재생하거나 목록을 확인할 수 있는 커맨드입니다.
    /// </summary>
    
    [Preserve]
    [ConsoleCommandClass]
    public class SoundCommand : IConsoleCommand
    {
        public string Command => "sound";
        public string Description => "sound play <soundName> - Plays a sound effect at the origin.\n " +
                                     "sound list - Lists all available sound names.";
        public string Signature => "sound play <soundName>\nsound list [cached|all]";

        private SoundManager SoundManager => CardevilCore.Instance.Sound;
        public void Execute(string[] args)
        {
            if (args.Length < 1)
            {
                Console.MessageInfo("Usage: sound play <soundName>");
                return;
            }

            string action = args[0];
            

            if (action == "play")
            {
                if (args.Length < 2)
                {
                    Console.MessageInfo("Usage: sound play <soundName>");
                    return;
                }
                string soundName = args[1];
                var emitter = SoundManager.PlaySfxAt(soundName, Vector3.zero);
                if (emitter != null)
                {
                    Console.MessageInfo($"Playing sound: {soundName}");
                }
                else
                {
                    Console.MessageError($"Sound not found: {soundName}");
                }
            }
            else if (action == "list")
            {
                string option = args.Length > 1 ? args[1] : "";
                if (option == "cached")
                {
                    var cachedSounds = SoundManager.GetCachedSoundNames();
                    Console.MessageInfo("Cached Sounds:");
                    foreach (var sound in cachedSounds)
                    {
                        Console.MessageInfo($"- {sound}");
                    }
                }
                else
                {
                    var allSounds = SoundManager.GetAllSoundNames();
                    Console.MessageInfo("All Available Sounds:");
                    foreach (var sound in allSounds)
                    {
                        Console.MessageInfo($"- {sound}");
                    }
                }
            }
            else
            {
                Console.MessageError($"Unknown action: {action}");
            }
        }
        
        public void AutoComplete(Span<string> args, ref List<string> suggestions)
        {
            switch (args.Length)
            {
                case 1:
                    if ("play".StartsWith(args[0], StringComparison.OrdinalIgnoreCase))
                    {
                        suggestions.Add("play");
                    }
                    if ("list".StartsWith(args[0], StringComparison.OrdinalIgnoreCase))
                    {
                        suggestions.Add("list");
                    }
                    break;
                case 2 when args[0].Equals("list", StringComparison.OrdinalIgnoreCase):
                    if ("cached".StartsWith(args[1], StringComparison.OrdinalIgnoreCase))
                    {
                        suggestions.Add("cached");
                    }
                    if ("all".StartsWith(args[1], StringComparison.OrdinalIgnoreCase))
                    {
                        suggestions.Add("all");
                    }
                    break;
                case 2 when args[0].Equals("play", StringComparison.OrdinalIgnoreCase):
                    if (SoundManager != null)
                    {
                        var allSounds = SoundManager.GetAllSoundNames();
                        foreach (var sound in allSounds)
                        {
                            if (sound.StartsWith(args[1], StringComparison.OrdinalIgnoreCase))
                            {
                                suggestions.Add(sound);
                            }
                        }
                    }
                    break;
            }
        }
    }
}