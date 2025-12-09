using UnityEngine;
using UnityEngine.UI;

namespace Cardevil.Cards.Visual.Base
{
    public class CardVisualCore : MonoBehaviour
    {
        [Header("Core")] 
        [SerializeField] protected Image innerFrame;
        [SerializeField] protected Image mainValue;
        [SerializeField] protected Image smallValue;
        
        public Image InnerFrame => innerFrame;
        public Image MainValue => mainValue;
        public Image SmallValue => smallValue;
    }
}