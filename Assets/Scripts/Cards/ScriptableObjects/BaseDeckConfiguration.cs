using Cardevil.Core;
using System.Collections.Generic;
using UnityEngine;

namespace Cardevil.Cards
{
    [CreateAssetMenu(menuName = "Cards/BaseDeckConfiguration")]
    public class BaseDeckConfiguration : ScriptableObject, IClearable
    {
        public List<NumberCardData> numberCardDatas = new(40);
        public List<DirectionCardData> directionCardDatas = new(10);

        public void Clear()
        {
            numberCardDatas.Clear();
            directionCardDatas.Clear();
        }
    }
}
