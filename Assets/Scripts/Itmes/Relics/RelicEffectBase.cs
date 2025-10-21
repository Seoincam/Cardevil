using Cardevil.Attributes;
using System;
using UnityEngine;

namespace Cardevil.Relics
{
    [Serializable]
    public abstract class RelicEffectBase
    {
        [SerializeField, VisibleOnly] protected Relic relic;

        [Header("Common")]
        [SerializeField, VisibleOnly] protected string inspectorDescription;
        [SerializeField, VisibleOnly] protected string effectId;

        public virtual void ActivateRelicEffect() { }
    }

