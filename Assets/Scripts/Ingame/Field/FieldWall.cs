using System;
using UnityEngine;

namespace Cardevil.Ingame.Field
{
    public class FieldWall : MonoBehaviour
    {
        private void OnEnable()
        {
            
        }
        private void OnDisable()
        {
            // Handle any cleanup or state reset when the wall is disabled
        }
        
        
        public void OnPlayerHealthChanged(float currentHealth, float maxHealth)
        {

        }
    }
}
