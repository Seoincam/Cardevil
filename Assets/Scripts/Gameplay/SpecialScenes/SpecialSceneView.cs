using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

namespace Cardevil.Gameplay.SpecialScenes
{
    public abstract class SpecialSceneView : MonoBehaviour
    {
        public event Action CloseRequested;

        protected void RaiseCloseRequested()
        {
            CloseRequested?.Invoke();
        }

        public abstract void Bind(SpecialSceneCore core);
        public abstract UniTask PlayEnterAsync();
        public abstract UniTask PlayExitAsync();
    }
}
