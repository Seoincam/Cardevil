using Cardevil.Attributes;
using UnityEngine;
using UnityEngine.UI;

namespace Cardevil.Core.Bootstrap
{
    public class BootstrapView : MonoBehaviour
    {
        [Header("UI")] 
        [SerializeField] private Slider progressBar;
        
        [Header("Loading")]
        [SerializeField, VisibleOnly] private int totalLoading;
        [SerializeField, VisibleOnly] private int loaded;

        private void Awake()
        {
            progressBar.value = 0f;
            Bootstrapper.ProgressChanged += OnProgressChanged;
        }

        private void OnProgressChanged(int loaded, int totalToLoad)
        {
            progressBar.value = Mathf.Clamp01((float)loaded / totalToLoad);
        }
    }
}
