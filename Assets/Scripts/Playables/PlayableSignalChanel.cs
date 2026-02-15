using Cardevil.Utils;
using System;
using UnityEngine;

namespace Cardevil.InGame.SlotMachine.Playables
{
    [CreateAssetMenu(fileName = "PlayableSignalChanel", menuName = "Cardevil/Playable/PlayableSignalChanel")]
    public class PlayableSignalChanel : ScriptableObject
    {
        public event Action<string> SignalStarted;
        public event Action<string> SignalEnded;
        
        
        public void InvokeSignalStarted(string signalName)
        {
            // LogEx.Log($"{name}: {signalName}");
            SignalStarted?.Invoke(signalName);
        }

        public void InvokeSignalEnded(string signalName)
        {
            // LogEx.Log($"{name}: {signalName}");
            SignalEnded?.Invoke(signalName);
        }
    }
}