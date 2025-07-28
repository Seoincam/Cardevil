using UnityEngine;
using UnityEngine.EventSystems;

namespace Cardevil.Cards.CardInteractinos
{
    public class TrashCan : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] CardBarGroup barGroup;
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (barGroup == null)
                return;

            barGroup.isOnTrashCan = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (barGroup == null)
                return;

            barGroup.isOnTrashCan = false;
        }
    }
}

