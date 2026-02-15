using System;
using UnityEngine;
using UnityEngine.Playables;

namespace Cardevil.InGame.SlotMachine.Playables
{
    [Serializable]
    public class ScriptableObjectSignalBehaviour : PlayableBehaviour
    {
        [Tooltip("시그널 식별자")]
        public string signalName;
    }
}