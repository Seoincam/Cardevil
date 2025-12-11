using Cardevil.Attributes;
using Cardevil.Manager;
using Cardevil.SceneManagement;
using Cardevil.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
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
            BootstrapFlow.ProgressChanged += OnProgressChanged;
        }

        private void OnProgressChanged(int loaded, int totalToLoad)
        {
            progressBar.value = Mathf.Clamp01((float)loaded / totalToLoad);
        }
    }
}
