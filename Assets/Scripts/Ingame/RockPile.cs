using Cardevil.Ingame.Entities;
using TMPro;
using UnityEngine;

namespace Cardevil.Ingame
{
    [RequireComponent(typeof(SpriteRenderer), typeof(Entity))]
    public class RockPile : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private TMP_Text countText;
        
        
        private int _rockCount;
        
        public int RockCount
        {
            get => _rockCount;
            set
            {
                _rockCount = value;
                countText.text = _rockCount.ToString();
            }
        }
    }
}