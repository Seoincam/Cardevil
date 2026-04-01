using UnityEngine;

namespace Cardevil.UI
{
    public class OverlayCanvas : MonoBehaviour
    {
        private static OverlayCanvas _instance;

        public static OverlayCanvas Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<OverlayCanvas>();
                }
                return _instance;
            }
        }

        private void Awake()
        {
            _instance = this;
        }
        
        
        [field:SerializeField] public TransitionPanel Transition{ get; set; }
        [field:SerializeField] public BlackPanel BlackPanel { get; set; }
    }
}