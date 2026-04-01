using System;
using UnityEngine;

namespace Cardevil.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class BlackPanel : MonoBehaviour
    {
        [field: SerializeField] public CanvasGroup CanvasGroup { get; private set; }
        private void Reset()
        {
            CanvasGroup = GetComponent<CanvasGroup>();
        }
        
        private void Awake()
        {
            OverlayCanvas.Instance.BlackPanel = this;
        }
    }
}