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
        
        /// <summary>
        /// 플레이어의 현재 체력
        /// </summary>
        /// <remarks>
        /// 값 설정 시 이벤트를 발생시켜 체력 변경을 알리며,
        /// 수정된 체력 값을 최종적으로 적용
        /// </remarks>
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
        
        /// <summary>
        /// 플레이어의 현재 체력을 강제로 설정합니다.
        /// </summary>
        /// <param name="hp">바꿀 hp</param>
        /// <param name="broadcast">이벤트 발생 여부. true시 이벤트는 발생하지만 변경된 값은 적용되지 않음</param>
        public void ForceSetCurrentHp(int hp, bool broadcast = true)
        {
            if (broadcast)
            {
                using(PlayerHealthChangeArgs args = PlayerHealthChangeArgs.Get())
                {
                    args.Init(_currentHP, hp);
                    Managers.Event.PlayerHealthChangeEvent.Invoke(args);
                    _currentHP = args.NewHealth;
                }
            }
            else
            {
                _currentHP = hp;
            }
        }
        
        public void BroadcastInitialStatus()
        {
            using(PlayerHealthChangeArgs args = PlayerHealthChangeArgs.Get())
            {
                args.Init(_currentHP, _currentHP);
                Managers.Event.PlayerHealthChangeEvent.Invoke(args);
            }
        }
    }
}