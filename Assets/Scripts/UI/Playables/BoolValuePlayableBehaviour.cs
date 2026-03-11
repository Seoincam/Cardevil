using System;
using UnityEngine;
using UnityEngine.Playables;

namespace Cardevil.InGame.SlotMachine.Playables
{
    [Serializable]
    public class BoolValuePlayableBehaviour : PlayableBehaviour
    {
        [Tooltip("클립 활성 구간 동안 적용할 값")]
        public bool value = true;
    }
}

