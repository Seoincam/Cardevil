using UnityEngine;
using System.Collections.Generic;

namespace Cardevil.Cards.CardInteractinos
{
    public class CardDeckVisual : MonoBehaviour
    {
        /*
        41-50
        31-40
        21-30
        11-20
        6-10
        5
        4
        3
        2
        1
        */

        [SerializeField] List<Transform> cards;

        public Transform FrontCardTransform => cards[0];
    }
}