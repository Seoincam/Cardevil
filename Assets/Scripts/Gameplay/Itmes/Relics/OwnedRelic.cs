using Cardevil.Attributes;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Cardevil.Relics
{
    [Serializable]
    public sealed class OwnedRelic
    {
        [SerializeField, VisibleOnly] private Relic relic;
        [SerializeField, VisibleOnly] private bool isActive = true;
        
        public Relic Relic => relic;
        public bool IsActive => isActive;
        public IReadOnlyList<RelicEffectBase> Effects => relic.Effects;
        
        public OwnedRelic(Relic relic) => this.relic = relic;
        public void SetActive(bool active) => isActive = active;
    }
}