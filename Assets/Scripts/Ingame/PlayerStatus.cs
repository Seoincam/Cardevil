using Cardevil.Core;
using Cardevil.Core.Bootstrap;
using Cardevil.DataStructure;
using Cardevil.DebugConsole;
using Cardevil.Events;
using Cardevil.Events.ExecEvents;
using Cardevil.Save;
using Cardevil.Utils;
using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using Console = Cardevil.DebugConsole.Console;

namespace Cardevil.Ingame
{
    /// <summary>
    /// 플레이어의 상태를 나타내는 클래스
    /// </summary>
    [Serializable]
    public class PlayerStatus : ISaveLoad, ICopy<PlayerStatus>, INewGameInitializable
    {
        // Hp
        [SerializeField] private int _currentHP;
        [SerializeField] private int _maxHP;
        [SerializeField] private int _shield;
        [SerializeField] public bool canRevive;
        
        // Card
        [SerializeField] private int _maxHand; // TODO: 실제 로직에 연결해야함.
        [SerializeField] private int _discardHand;
        
        // Current
        [SerializeField] private int _rerollTicket;
        [SerializeField] private int _gold;
        [SerializeField] public int _slotMachineLevel;
        
        [SerializeField] private VariableContainer _variableContainer = new VariableContainer();

        /*
         * TODO: 추가로 저장해야할 것
         * 유물 상태,
         * 아이템 상태
         */
        
        
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
                PlayerHealthChangeArgs args = PlayerHealthChangeArgs.Get();
                
                args.Init(_currentHP, value);
                ExecEventBus<PlayerHealthChangeArgs>.InvokeMergedAndDispose(args).Forget();
                _currentHP = args.ModifiedHealth;
                
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
                PlayerShieldChangeArgs args = PlayerShieldChangeArgs.Get();
                
                args.Init(_shield, value);
                ExecEventBus<PlayerShieldChangeArgs>.InvokeMergedAndDispose(args).Forget();
                _shield = args.ModifiedShield;
                
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
                RerollTicketChangeArgs args = RerollTicketChangeArgs.Get();
                args.Init(_rerollTicket, value);
                ExecEventBus<RerollTicketChangeArgs>.InvokeMergedAndDispose(args).Forget();
                _rerollTicket = args.ModifiedTicket;
            }
        }
        
        public VariableContainer VariableContainer => _variableContainer;
        
        public int MaxHand
        {
            get => _maxHand;
            set => _maxHand = value;
        }
        
        public int DiscardCard
        {
            get => _discardHand;
            set => _discardHand = value;
        }
        public int TakeDamage(int damage)
        {
            if (damage < 0)
            {
                Debug.LogWarning("Damage cannot be negative.");
                LogEx.LogWarning("Damage cannot be negative.");
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
        
        public void Heal(int amount)
        {
            if (amount < 0)
            {
                LogEx.LogWarning("Heal amount cannot be negative.");
                return;
            }
            CurrentHp = Math.Min(CurrentHp + amount, MaxHp);
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
                PlayerHealthChangeArgs args = PlayerHealthChangeArgs.Get();
                
                args.Init(_currentHP, hp);
                ExecEventBus<PlayerHealthChangeArgs>.InvokeMergedAndDispose(args).Forget();
                _currentHP = args.NewHealth;
                
            }
            else
            {
                _currentHP = hp;
            }
        }
        
        public void BroadcastInitialStatus()
        {
            PlayerHealthChangeArgs args = PlayerHealthChangeArgs.Get();
            args.Init(_currentHP, _currentHP);
            args.IsJustBroadcast = true;
            ExecEventBus<PlayerHealthChangeArgs>.InvokeMergedAndDispose(args).Forget();
        }
        
        public void SetUpNewGame(GameSave currentSave)
        {
            // TODO: 나중에 SO 등 다른 방법으로 개선
            
            // Hp
            _currentHP = 3;
            _maxHP = 3;
            _shield = 0;
            canRevive = false;
            
            // Card
            _maxHand = 6;
            _discardHand = 3;
            
            // Current
            _rerollTicket = 0;
            _gold = 0;
            _slotMachineLevel = 1;
        }

        public void Save(GameSave currentSave)
        {
            currentSave.PlayerStatus.CopyFrom(this);
        }

        public void Load(GameSave currentSave)
        {
            currentSave.PlayerStatus.CopyTo(this);
            BroadcastInitialStatus();
        }

        public void CopyFrom(PlayerStatus other)
        {
            // Hp
            _currentHP = other._currentHP;
            _maxHP = other._maxHP;
            _shield = other._shield;
            canRevive = other.canRevive;
            
            // Card
            _maxHand = other._maxHand;
            _discardHand = other._discardHand;
            
            //Current
            _rerollTicket = other._rerollTicket;
            _gold = other._gold;
            _slotMachineLevel = other._slotMachineLevel;
            
            _variableContainer.CopyFrom(other._variableContainer);
        }

        public void CopyTo(PlayerStatus other)
        {
            other.CopyFrom(this);
        }

        #region Console Commands
        
        [ConsoleCommand("heal", "Heal the player by a specified amount.","heal [int: amount (optional, default: 1)]", new []{"0","1","2","3"})]
        private static void HealCommand(string[] args)
        {
            int amount;
            if(args.Length == 0)
            {
                amount = 1;
            }
            else
            {
                if (!int.TryParse(args[0], out amount))
                {
                    DebugConsole.Console.MessageError("Invalid heal amount. Please provide a valid integer.");
                    return;
                }
            }
            if (amount < 0)
            {
                DebugConsole.Console.MessageWarning("Heal amount cannot be negative.");
                return;
            }
            CardevilCore.Instance.Game.PlayerStatus.Heal(amount);
            DebugConsole.Console.MessageInfo($"Healed {amount} HP. Current HP: {CardevilCore.Instance.Game.PlayerStatus.CurrentHp}/{CardevilCore.Instance.Game.PlayerStatus.MaxHp}");
        }

        [ConsoleCommand("deal", "Deal damage to the player by a specified amount.", "deal [int: amount]", new []{"0","1","2","3"})]
        private static void DealCommand(string[] args)
        {
            int amount;
            if (args.Length == 0)
            {
                amount = 1;
            }
            else
            {
                if (!int.TryParse(args[0], out amount))
                {
                    DebugConsole.Console.MessageError("Invalid damage amount. Please provide a valid integer.");
                    return;
                }
            }
            if (amount < 0)
            {
                DebugConsole.Console.MessageWarning("Damage amount cannot be negative.");
                return;
            }
            int actualDamage = CardevilCore.Instance.Game.PlayerStatus.TakeDamage(amount);
            DebugConsole.Console.MessageInfo($"Dealt {actualDamage} damage. Current HP: {CardevilCore.Instance.Game.PlayerStatus.CurrentHp}/{CardevilCore.Instance.Game.PlayerStatus.MaxHp}");
        }

        [ConsoleCommand("sethp", "플레이이어의 HP를 설정합니다.","sethp <int: amount> [bool: broadcast (optional, default: true)]", new []{"0","1","2","3"})]
        private static void SetHpCommand(string[] args)
        {
            if (args.Length == 0)
            {
                DebugConsole.Console.MessageError(Console.Instance.CurrentCommand.Description);
                return;
            }
            
            if(args.Length == 1)
            {
                int hp = CommandHelper.ParseArgument<int>(args[0]);
                CardevilCore.Instance.Game.PlayerStatus.ForceSetCurrentHp(hp, true);
                DebugConsole.Console.MessageInfo($"Set player HP to {CardevilCore.Instance.Game.PlayerStatus.CurrentHp}/{CardevilCore.Instance.Game.PlayerStatus.MaxHp}");
            }
            else
            {
                int hp = CommandHelper.ParseArgument<int>(args[0]);
                bool doBroadcast = CommandHelper.ParseArgument<bool>(args[1]);
                CardevilCore.Instance.Game.PlayerStatus.ForceSetCurrentHp(hp, doBroadcast);
                DebugConsole.Console.MessageInfo($"Set player HP to {CardevilCore.Instance.Game.PlayerStatus.CurrentHp}/{CardevilCore.Instance.Game.PlayerStatus.MaxHp} with broadcast: {doBroadcast}");
            }
        }

        #endregion
    }
}