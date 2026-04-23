using Cardevil.Card.Common.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Cardevil.Card.InWorld
{
    public class HandRankDescriptionRow : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI bonusDamageText;

        public Button Button => button;

        public string HandRank
        {
            // TODO: 실제 족보명이랑 연결
            set => nameText.text = value;
        }

        public int Damage
        {
            // TODO: 실제 족보 데미지랑 연결
            set => bonusDamageText.text = value.ToString();
        }
    }
}