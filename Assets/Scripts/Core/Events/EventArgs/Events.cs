using Cardevil.Core.Events.ExecEvent;
using Cardevil.Gameplay;
using Cardevil.Gameplay.Enemy;

namespace Cardevil.Core.Events.EventArgs
{
    
    /// <summary>
    /// 각 특정 상태변화가 끝난후, 해당 상태변화에 대한 정보를 담는 이벤트 아규먼트입니다.
    /// </summary>
    public class PlayerStatusChangedArgs : ExecEventArgs<PlayerStatusChangedArgs>
    {
        public enum Priority
        {
            First = int.MinValue,
            UI = 1000,
        }

        public bool IsJustBroadcast { get; set; } = false;
        
        public StatType StatType { get; private set; }
        public int OldValue { get; private set; }
        public int NewValue { get; private set; }
        
        public static PlayerStatusChangedArgs Get(StatType statType, int currentValue, int newValue)
        {
            var args = Get();
            args.StatType = statType;
            args.OldValue = currentValue;
            args.NewValue = newValue;
            return args;
        }
    }
    
    public class PlayerHealthChangeArgs : ExecEventArgs<PlayerHealthChangeArgs>
    {
        public enum Priority
        {
            First = int.MinValue,
            UI = 1000,
        }

        public bool IsJustBroadcast { get; set; } = false;
        public int OldHealth { get; private set; }
        public int NewHealth { get; private set; }

        public static PlayerHealthChangeArgs Get(int currentHealth, int newHealth)
        {
            var args = Get();
            args.OldHealth = currentHealth;
            args.NewHealth = newHealth;
            return args;
        }

        public override void Clear()
        {
            base.Clear();
            IsJustBroadcast = false;
            OldHealth = 0;
            NewHealth = 0;
        }
    }

    public class PlayerShieldChangeArgs : ExecEventArgs<PlayerShieldChangeArgs>
    {
        public int OldShield { get; private set; }
        public int NewShield { get; private set; }
        public int ModifiedShield { get; set; }

        public static PlayerShieldChangeArgs Get(int currentShield, int newShield)
        {
            var args = Get();
            args.OldShield = currentShield;
            args.NewShield = newShield;
            return args;
        }

        public override void Clear()
        {
            base.Clear();
            OldShield = 0;
            NewShield = 0;
        }
    }

    public class PlayerGoldChangeArgs : ExecEventArgs<PlayerGoldChangeArgs>
    {
        public enum Priority
        {
            First = int.MinValue,
            UI = 1000,
        }

        public bool IsJustBroadcast { get; set; } = false;
        public int OldGold { get; private set; }
        public int NewGold { get; private set; }

        public static PlayerGoldChangeArgs Get(int currentGold, int newGold)
        {
            var args = Get();
            args.OldGold = currentGold;
            args.NewGold = newGold;
            return args;
        }

        public override void Clear()
        {
            base.Clear();
            IsJustBroadcast = false;
            OldGold = 0;
            NewGold = 0;
        }
    }

    public class RerollTicketChangeArgs : ExecEventArgs<RerollTicketChangeArgs>
    {
        public int OldTicket { get; private set; }
        public int NewTicket { get; private set; }
        public int ModifiedTicket { get; set; }

        public void Init(int currentTicket, int newTicket)
        {
            OldTicket = currentTicket;
            NewTicket = newTicket;
            ModifiedTicket = newTicket;
        }

        public override void Clear()
        {
            base.Clear();
            OldTicket = 0;
            NewTicket = 0;
            ModifiedTicket = 0;
        }
    }

    public class EnemyHealthChangeArgs : ExecEventArgs<EnemyHealthChangeArgs>
    {
        public float OldHealth { get; private set; }
        public float NewHealth { get; private set; }
        public float ModifiedHealth { get; set; }
        public Enemy Owner;

        public void Init(float currentHealth, float newHealth, Enemy owner)
        {
            OldHealth = currentHealth;
            NewHealth = newHealth;
            ModifiedHealth = newHealth;
            Owner = owner;
        }

        public override void Clear()
        {
            base.Clear();
            OldHealth = 0;
            NewHealth = 0;
            ModifiedHealth = 0;
            Owner = null;
        }
    }

    public class EnemyAttackAfterArgs : ExecEventArgs<EnemyAttackAfterArgs>
    {
        public bool isPlayerAttackSuccess;

        public override void Clear()
        {
            base.Clear();
            isPlayerAttackSuccess = false;
        }
    }

    public class EnemyTurnEndArgs : ExecEventArgs<EnemyTurnEndArgs>
    {
        public override void Clear()
        {
            base.Clear();
        }
    }
}
