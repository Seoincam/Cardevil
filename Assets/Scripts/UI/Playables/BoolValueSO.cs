using UnityEngine;

namespace Cardevil.UI.Playables
{
    [CreateAssetMenu(fileName = "BoolValue", menuName = "Cardevil/Playable/BoolValue")]
    public class BoolValueSO : ScriptableObject
    {
        [SerializeField] private bool value;

        public bool Value
        {
            get => value;
            set => this.value = value;
        }
    }
}

