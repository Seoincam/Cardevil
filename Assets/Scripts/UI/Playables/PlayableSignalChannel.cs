using Cardevil.Core.Utils;
using System;
using UnityEngine;

namespace Cardevil.UI.Playables
{
    [CreateAssetMenu(fileName = "PlayableSignalChanel", menuName = "Cardevil/Playable/PlayableSignalChanel")]
    public class PlayableSignalChannel : ScriptableObject
    {
        [field: SerializeField] public bool IsGlobal { get; private set; }
        
        public static PlayableSignalChannel Global { get; private set; }
        
        public event Action<string> SignalStarted;
        public event Action<string> SignalEnded;

        private void OnEnable()
        {
            if (IsGlobal)
            {
                if (Global != null)
                {
                    LogEx.LogWarning($"PlayableSignalChannel: Global channel already exists. Overriding with {name}.");
                }
                Global = this;
            }
        }
        
        private void OnDisable()
        {
            if (IsGlobal)
            {
                Global = null;
            }
        }

        private void OnValidate()
        {
            if (IsGlobal && Global != null && Global != this)
            {
                LogEx.LogWarning($"PlayableSignalChannel: Global channel already exists. Overriding with {name}.");
                Global.IsGlobal = false;
                Global = this;
            }
        }

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