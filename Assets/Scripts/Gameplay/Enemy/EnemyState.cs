using System;
using UnityEngine;

namespace Cardevil.Gameplay.Enemy
{
    public class EnemyState : MonoBehaviour
    {
        public Enemy enemy;

        public string name;
        public int maxHealth;
        public int _currentHealth;

        public event Action<int, int> OnHealthChanged;

        public EnemyState(string name,int health)
        {
            this.name = name;
            this.maxHealth = health;
            this.currentHealth = health;
        }

        public int currentHealth
        {
            get => _currentHealth;
            set
            {
                _currentHealth = Math.Max(0, value);
                OnHealthChanged?.Invoke(_currentHealth, maxHealth);
            }
        }

        
    }
}
