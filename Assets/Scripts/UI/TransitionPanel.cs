using System;
using UnityEngine;

namespace Cardevil.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class TransitionPanel : MonoBehaviour
    {
        [field: SerializeField] public CanvasGroup CanvasGroup { get; private set; }
        
        private void Reset()
        {
            CanvasGroup = GetComponent<CanvasGroup>();
        }

        private void Awake()
        {
            OverlayCanvas.Instance.Transition = this;
        }
    }
}