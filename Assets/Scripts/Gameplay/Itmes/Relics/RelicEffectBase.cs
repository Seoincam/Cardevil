using Cardevil.Attributes;
using System;
using UnityEngine;

namespace Cardevil.Relics
{
    [Serializable]
    public abstract class RelicEffectBase
    {
        [Header("Common")]
        [SerializeField, VisibleOnly] protected Relic relic;
        [SerializeField, VisibleOnly] protected string effectId;

        public virtual void ActivateRelicEffect() { }
    }

}
