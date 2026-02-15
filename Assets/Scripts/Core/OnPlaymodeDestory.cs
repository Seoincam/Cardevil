using UnityEngine;

namespace Cardevil.Core
{
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(-100000)]
    public class OnPlaymodeDestory : MonoBehaviour
    {
        private void OnDestroy()
        {
            if (Application.isPlaying)
            {
                Destroy(gameObject);
            }
        }
    }
}