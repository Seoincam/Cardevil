using Cardevil.Events;
using System;
using UnityEngine;

namespace Cardevil.Ingame.Field
{
    public class FieldWall : MonoBehaviour
    {
        private void OnEnable()
        {
            Managers.Event.PlayerHealthChangeEvent.AddListener(OnPlayerHealthChanged,10);
        }
        private void OnDisable()
        {
            Managers.Event.PlayerHealthChangeEvent.RemoveListener(OnPlayerHealthChanged,10);
        }
        
        
        public void OnPlayerHealthChanged(PlayerHealthChangeArgs args)
        {
            Debug.Log($"Player health changed: {args.OldHealth} -> {args.NewHealth}");
        }

    }
}
