using Cardevil.DataStructure;
using Cardevil.Events;
using System;
using UnityEngine;

namespace Cardevil.Ingame
{
    /// <summary>
    /// 플레이어의 상태를 나타내는 클래스
    /// </summary>
    [Serializable]
    public class PlayerStatus
    {
        [SerializeField] private int _currentHP = 3;
        [SerializeField] private int _maxHP = 3;
        [SerializeField] private int _shield = 0;
        [SerializeField] private int _rerollTicket = 0;
        [SerializeField] private VariableContainer _variableContainer = new VariableContainer();
        
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
        
        public int Shield
        {
            get => _shield;
            set 
            {
                using(PlayerShieldChangeArgs args = PlayerShieldChangeArgs.Get())
                {
                    args.Init(_shield, value);
                    Managers.Event.PlayerShieldChangeEvent.Invoke(args);
                    _shield = args.ModifiedShield;
                }
            }
        }

        /// <summary>
        /// 시작 카드 뽑기권의 남은 개수
        /// </summary>
        /// <remarks>
        /// 개수 변경 시 이벤트를 발생시켜 변경을 알리며,
        /// 수정된 개수를 값을 최종적으로 적용
        /// </remarks>
        public int RerollTicket
        {
            get => _rerollTicket;
            set
            {
                using (RerollTicketChangeArgs args = RerollTicketChangeArgs.Get())
                {
                    args.Init(_rerollTicket, value);
                    Managers.Event.RerollTicketChangeEvent.Invoke(args);
                    _rerollTicket = args.ModifiedTicket;
                }
            }
        }
        
        public VariableContainer VariableContainer => _variableContainer;
        
        
        public int TakeDamage(int damage)
        {
            if (damage < 0)
            {
                Debug.LogWarning("Damage cannot be negative.");
                return 0;
            }
            int oldHp = CurrentHp;
            
            if (Shield >= damage)
            {
                Shield -= damage;
                damage = 0;
            }
            else
            {
                damage -= Shield;
                Shield = 0;
            }
            CurrentHp -= damage;
            return oldHp - _currentHP; // 실제로 감소한 체력 반환
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