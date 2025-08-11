using Cardevil.Events;
using System;
using UnityEngine;

namespace Cardevil.Ingame.Field
{
    public class FieldWall : MonoBehaviour
    {
        [SerializeField] private GameObject[] _walls;
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
            for (int i = 0; i < _walls.Length; i++)
            {
                if (_walls[i] != null)
                {
                    bool isActive = i < args.ModifiedHealth;
                    _walls[i].SetActive(isActive);
                }
            }
        }

    }
}
