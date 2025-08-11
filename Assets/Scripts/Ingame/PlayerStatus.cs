using Cardevil.Events;
using System;

namespace Cardevil.Ingame
{
    /// <summary>
    /// 플레이어의 상태를 나타내는 클래스
    /// </summary>
    [Serializable]
    public class PlayerStatus
    {
        private int _currentHP = 3;
        private int _maxHP = 3;
        
        public int CurrentHp
        {
            get => _currentHP;
            set
            {
                using(PlayerHealthChangeArgs args = PlayerHealthChangeArgs.Get())
                {
                    args.Init(_currentHP, value);
                    Managers.Event.PlayerHealthChangeEvent.Invoke(args);
                    _currentHP = args.ModifiedHealth;
                }
                
            }
        }
        public int MaxHp
        {
            get => _maxHP;
            set => _maxHP = value;
        }
    }
}