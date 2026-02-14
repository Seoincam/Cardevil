using Cardevil.NewCard.Common.Visual;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Cardevil.NewCard.Visual.Controller
{
    public class ColorJewelDecoration : MonoBehaviour
    {
        [Header("Prefabs")] 
        [SerializeField] private GameObject colorJewelPrefab;
        
        [Header("References")]
        [SerializeField] private Transform jewelAnchor;

        [Header("Settings")] 
        [SerializeField] private float jewelSpacing = 0.1f;
        
        private List<GameObject> _colorJewels;

        private void Awake()
        {
            _colorJewels = new List<GameObject>();
        }

        public void Apply(in CardDecorationData data)
        {
            if (_colorJewels != null)
            {
                foreach (GameObject colorJewel in _colorJewels)
                {
                    Destroy(colorJewel);
                }
                _colorJewels.Clear();
            }

            for (int i = 0; i < data.JewelSprites.Length; i++)
            {
                GameObject colorJewel = Instantiate(colorJewelPrefab, jewelAnchor);
                var spriteRenderer = colorJewel.GetComponent<SpriteRenderer>();
                spriteRenderer.sprite = data.JewelSprites[i];
                
                var x = GetJewelX(i,  data.JewelSprites.Length);
                colorJewel.transform.localPosition = new Vector3(x, 0f);
                
                _colorJewels.Add(colorJewel);
            }
        }

        public void SetSortingOrder(int sortingOrder)
        {
            if (_colorJewels == null) return;
            
            for (int i = 0; i < _colorJewels.Count; i++)
            {
                _colorJewels[i].GetComponent<SpriteRenderer>().sortingOrder = 100 * sortingOrder + 80 + i;
            }
        }

        private float GetJewelX(int index, int jewelCount)
        {
            if (jewelCount == 1) return 0f;
            
            return (index - (jewelCount - 1) * 0.5f) * jewelSpacing;
        }
    }
}