using UnityEngine;

namespace Cardevil.Gameplay.Animation
{
    public class AnimSignalListener : MonoBehaviour, IAnimSignalListener
    {
        public event System.Action<string> SignalEvent;
        public void OnSignalEvent(string eventName)
        {
            SignalEvent?.Invoke(eventName);
        }
    }
}