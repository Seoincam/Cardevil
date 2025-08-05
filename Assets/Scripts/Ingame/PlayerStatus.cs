using System;

namespace Cardevil.Ingame
{
    [Serializable]
    public class PlayerStatus
    {
        private float _currentHP;
        private float _maxHP;
        
        public float CurrentHp
        {
            get => _currentHP;
            set => _currentHP = value;
        }
        public float MaxHp
        {
            get => _maxHP;
            set => _maxHP = value;
        }
    }
}