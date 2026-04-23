using UnityEngine;

namespace Cardevil.Card.Visual.Controller
{
    public class UICardVisualController : CardVisualControllerBase
    {
        // UI-specific logic goes here. 
        // e.g. using Canvas Group for DOFade instead of DOTweening individual components,
        // or modifying sorting via UI Hierarchy.

        public override void SetSortingOrder(int sortingOrder, CardLayer layer = CardLayer.Default)
        {
            // For UI, we may not need to set SortingLayerID. Hierarchy determines order.
            // If overriding using a Canvas component:
            /*
            var canvas = GetComponent<Canvas>();
            if (canvas)
            {
                canvas.overrideSorting = true;
                canvas.sortingOrder = sortingOrder;
            }
            */
            
            // Still call base to allow ICardRenderer logic to run if implemented for UI
            base.SetSortingOrder(sortingOrder, layer);
        }
    }
}
