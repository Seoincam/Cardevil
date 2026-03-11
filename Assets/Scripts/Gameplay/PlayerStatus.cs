using Cardevil.Core;
using Cardevil.Core.Bootstrap;
using Cardevil.Core.DataStructure;
using Cardevil.Core.DataStructure.Serializables;
using Cardevil.Core.Events.EventArgs;
using Cardevil.Core.Events.ExecEvent;
using Cardevil.Core.Systems.Save;
using Cardevil.Core.Utils;
using Cardevil.Gameplay.Relic.Core;
using Cardevil.Test.DebugConsole;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;
using Console = Cardevil.Test.DebugConsole.Console;

namespace Cardevil.Gameplay
{
    public enum PlayerStatType
    {
        Level,
        SlotMachineLevel,
        
        MaxHp,
        CurrentHp,
        Shield,
        
        MaxHandCount,
        DefaultDiscardCount,
        
        Gold,
        RerollTicket,
    }
    
    /// <summary>
    /// 플레이어의 상태를 나타내는 클래스
    /// </summary>
    [Serializable]
    public class PlayerStatus : ISaveLoad, ICopy<PlayerStatus>, INewGameInitializable
    {
        [SerializeField] private SerializableDictionary<PlayerStatType, StatEntry> stats = new();
        [SerializeReference] private SerializableDictionary<PlayerStatType, List<IStatModifier>> cachedModifiers = new();
        
        [SerializeField] private VariableContainer _variableContainer = new VariableContainer();

        private int _nextId;
        private Dictionary<int, IStatModifier> _modifiers = new();

        private static PlayerStatConfig _config;

        /// <summary>
        /// 플레이어의 현재 체력
        /// </summary>
        /// <remarks>
        /// 값 설정 시 이벤트를 발생시켜 체력 변경을 알리며,
        /// 수정된 체력 값을 최종적으로 적용
        /// </remarks>
        public int CurrentHp
        {
            get => GetFinalValue(PlayerStatType.CurrentHp);
            set => SetBaseValue(PlayerStatType.CurrentHp, value);
        }
        public int MaxHp
        {
            get => GetFinalValue(PlayerStatType.MaxHp);
            set
            {
                SetBaseValue(PlayerStatType.MaxHp, value);
                CurrentHp = Mathf.Clamp(CurrentHp, 0, MaxHp);
            }
        }
        
        public int Shield
        {
            get => GetFinalValue(PlayerStatType.Shield);
            set => SetBaseValue(PlayerStatType.Shield, value);
        }
        
        public VariableContainer VariableContainer => _variableContainer;
        
        /// <summary>
        /// 스탯의 <c>BaseValue</c>를 설정합니다.
        /// </summary>
        public void SetBaseValue(PlayerStatType playerStatType, int value)
        {
            GetOrAddStat(playerStatType).SetBaseValue(value);
            RecalculateAndNotify(playerStatType);
        }

        /// <summary>
        /// 스탯의 <c>BaseValue</c> + <c>delta</c>를 수행합니다.
        /// </summary>
        public void ModifyBaseValue(PlayerStatType playerStatType, int delta)
        {
            var stat = GetOrAddStat(playerStatType);
            stat.SetBaseValue(stat.BaseValue + delta);
            RecalculateAndNotify(playerStatType);
        }
        
        /// <summary>
        /// 스탯의 <c>BaseValue</c>를 반환함. 해당 스탯 엔트리가 존재하지 않을 경우, 추가함.
        /// </summary>
        public int GetBaseValue(PlayerStatType playerStatType)
        {
            return GetOrAddStat(playerStatType).BaseValue;
        }

        /// <summary>
        /// 스탯의 <c>FinalValue</c>를 반환함. 해당 스탯 엔트리가 존재하지 않을 경우, 추가함.
        /// </summary>
        public int GetFinalValue(PlayerStatType playerStatType)
        {
            return GetOrAddStat(playerStatType).FinalValue;
        }

        /// <summary>
        /// 수정자를 추가. Base Value에 영향을 주지 않고 일시적인 변경에 사용됨.
        /// </summary>
        /// <param name="modifier"></param>
        /// <returns></returns>
        public int AddModifier(IStatModifier modifier)
        {
            int modifierId = _nextId++;
            
            if (!cachedModifiers.TryGetValue(modifier.TargetType, out var list))
            {
                list = new List<IStatModifier>();
                cachedModifiers.Add(modifier.TargetType, list);
            }
            list.Add(modifier);
            _modifiers.Add(modifierId, modifier);
            
            RecalculateAndNotify(modifier.TargetType);

            return modifierId;
        }

        /// <summary>
        /// Id와 수정자 객체를 검증 후 제거함.
        /// </summary>
        public void SafeRemoveModifier(int modifierId, IStatModifier modifierToRemove)
        {
            if (!_modifiers.TryGetValue(modifierId, out var modifier))
            {
                LogEx.LogWarning($"modifierId({modifierId})에 해당하는 Modifier가 존재하지 않음.");
                return;
            }

            if (modifier != modifierToRemove)
            {
                LogEx.LogError("ModifierId로 찾은 Modifier와 인자로 받은 Modifier가 일치하지 않음.");
                return;
            }

            if (cachedModifiers.TryGetValue(modifier.TargetType, out var list))
            {
                list.Remove(modifier);
            }
            _modifiers.Remove(modifierId);
            
            RecalculateAndNotify(modifierToRemove.TargetType);
        }
        
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
            return oldHp - CurrentHp; // 실제로 감소한 체력 반환
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
                var args = PlayerHealthChangeArgs.Get(CurrentHp, hp);
                ExecEventBus<PlayerHealthChangeArgs>.InvokeMergedAndDispose(args).Forget();
                CurrentHp = args.NewHealth;
                
            }
            else
            {
                CurrentHp = hp;
            }
        }
        
        public void BroadcastInitialStatus()
        {
            var args = PlayerHealthChangeArgs.Get(CurrentHp, CurrentHp);
            args.IsJustBroadcast = true;
            ExecEventBus<PlayerHealthChangeArgs>.InvokeMergedAndDispose(args).Forget();
        }
        
        public void SetUpNewGame(GameSave currentSave)
        {
            if (!_config)
            {
                var configs = Resources.LoadAll<PlayerStatConfig>("");
                if (configs.Length > 0)
                {
                    _config = configs[0];
                }
                else
                {
                    LogEx.LogError("PlayerStatConfig not found in Resources folder.");
                }

                Debug.Assert(_config, "_config != null");
            }
            
            foreach (var entry in _config.Entries)
            {
                SetBaseValue(entry.Type, entry.Value);
            }
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
            foreach (var (type, stat) in other.stats)
            {
                SetBaseValue(type, stat.BaseValue);
            }

            _variableContainer.CopyFrom(other._variableContainer);
        }

        public void CopyTo(PlayerStatus other)
        {
            other.CopyFrom(this);
        }
        
        private StatEntry GetOrAddStat(PlayerStatType type)
        {
            if (!stats.TryGetValue(type, out var stat))
            {
                stat = new StatEntry(type);
                stats.Add(type, stat);
            }
            return stat;
        }

        private void RecalculateAndNotify(PlayerStatType type)
        {
            var stat = GetOrAddStat(type);
            int previousFinalValue = stat.FinalValue;

            int currentFinalValue = stat.BaseValue;
            if (cachedModifiers.TryGetValue(type, out var list))
            {
                foreach (var modifier in list)
                {
                    currentFinalValue = modifier.Modify(currentFinalValue);
                }
            }
            
            // 값이 바뀌었는지 체크
            if (previousFinalValue != currentFinalValue)
            {
                stat.SetFinalValue(currentFinalValue);
                Notify(type, previousFinalValue, currentFinalValue);
            }
        }

        private void Notify(PlayerStatType statType, int previous, int current)
        {
            switch (statType)
            {
                case PlayerStatType.MaxHp:
                    CurrentHp = Mathf.Clamp(CurrentHp, 0, current);
                    break;
                
                case PlayerStatType.CurrentHp:
                    var hpArgs = PlayerHealthChangeArgs.Get(previous, current);
                    ExecEventBus<PlayerHealthChangeArgs>.InvokeMergedAndDispose(hpArgs).Forget();
                    break;
                
                case PlayerStatType.Shield:
                    var shieldArgs = PlayerShieldChangeArgs.Get(previous, current);
                    ExecEventBus<PlayerShieldChangeArgs>.InvokeMergedAndDispose(shieldArgs).Forget();
                    break;
            }
        }

        [Serializable]
        private class StatEntry
        {
            [field: SerializeField] public PlayerStatType Type { get; private set; }
            [field: SerializeField] public int BaseValue { get; private set; }
            [field: SerializeField] public int FinalValue { get; private set; }
            public StatEntry(PlayerStatType type) => Type = type;
            public void SetBaseValue(int value) => BaseValue = value;
            public void SetFinalValue(int value) => FinalValue = value;
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
                    Console.MessageError("Invalid heal amount. Please provide a valid integer.");
                    return;
                }
            }
            if (amount < 0)
            {
                Console.MessageWarning("Heal amount cannot be negative.");
                return;
            }
            CardevilCore.PlayerStatus.Heal(amount);
            Console.MessageInfo($"Healed {amount} HP. Current HP: {CardevilCore.PlayerStatus.CurrentHp}/{CardevilCore.Game.PlayerStatus.MaxHp}");
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
                    Console.MessageError("Invalid damage amount. Please provide a valid integer.");
                    return;
                }
            }
            if (amount < 0)
            {
                Console.MessageWarning("Damage amount cannot be negative.");
                return;
            }
            int actualDamage = CardevilCore.PlayerStatus.TakeDamage(amount);
            Console.MessageInfo($"Dealt {actualDamage} damage. Current HP: {CardevilCore.PlayerStatus.CurrentHp}/{CardevilCore.PlayerStatus.MaxHp}");
        }

        [ConsoleCommand("sethp", "플레이이어의 HP를 설정합니다.","sethp <int: amount> [bool: broadcast (optional, default: true)]", new []{"0","1","2","3"})]
        private static void SetHpCommand(string[] args)
        {
            if (args.Length == 0)
            {
                Console.MessageError(Console.Instance.CurrentCommand.Description);
                return;
            }
            
            if(args.Length == 1)
            {
                int hp = CommandHelper.ParseArgument<int>(args[0]);
                CardevilCore.PlayerStatus.ForceSetCurrentHp(hp, true);
                Console.MessageInfo($"Set player HP to {CardevilCore.PlayerStatus.CurrentHp}/{CardevilCore.PlayerStatus.MaxHp}");
            }
            else
            {
                int hp = CommandHelper.ParseArgument<int>(args[0]);
                bool doBroadcast = CommandHelper.ParseArgument<bool>(args[1]);
                CardevilCore.PlayerStatus.ForceSetCurrentHp(hp, doBroadcast);
                Console.MessageInfo($"Set player HP to {CardevilCore.PlayerStatus.CurrentHp}/{CardevilCore.PlayerStatus.MaxHp} with broadcast: {doBroadcast}");
            }
        }

        #endregion
    }
}