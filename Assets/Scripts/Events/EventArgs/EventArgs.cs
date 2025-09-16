namespace Cardevil.Events
{
    /// <summary>
    /// 플레이어의 체력 변화 이벤트 인자.
    /// </summary>
    public class PlayerHealthChangeArgs : EventArgs<PlayerHealthChangeArgs>
    {
        /// <summary>
        /// 단순히 체력 변화 이벤트를 방송만 할 때 true로 설정.
        /// </summary>
        public bool IsJustBroadcast {get; set;} = false;
        public int OldHealth { get; private set; }
        public int NewHealth { get; private set; }

        /// <summary>
        /// 이벤트 진행으로 인해 수정된 체력 값. 최종적으로 해당 값으로 플레이어의 체력이 설정됨.
        /// </summary>
        public int ModifiedHealth { get; set; }

        public void Init(int currentHealth, int newHealth)
        {
            OldHealth = currentHealth;
            NewHealth = newHealth;
            ModifiedHealth = newHealth;
        }

        public override void Clear()
        {
            OldHealth = 0;
            NewHealth = 0;
            ModifiedHealth = 0;
        }
    }

    /// <summary>
    /// 플레이어의 방어막 변화 이벤트 인자.
    /// </summary>
    public class PlayerShieldChangeArgs : EventArgs<PlayerShieldChangeArgs>
    {
        public int OldShield { get; private set; }
        public int NewShield { get; private set; }

        /// <summary>
        /// 이벤트 진행으로 인해 수정된 방어막 값. 최종적으로 해당 값으로 플레이어의 방어막이 설정됨.
        /// </summary>
        public int ModifiedShield { get; set; }

        public void Init(int currentShield, int newShield)
        {
            OldShield = currentShield;
            NewShield = newShield;
            ModifiedShield = newShield;
        }

        public override void Clear()
        {
            OldShield = 0;
            NewShield = 0;
            ModifiedShield = 0;
        }
    }

    /// <summary>
    /// 시작 카드 뽑기권 개수 변화 이벤트 인자.
    /// </summary>
    public class RerollTicketChangeArgs : EventArgs<RerollTicketChangeArgs>
    {
        public int OldTicket { get; private set; }
        public int NewTicket { get; private set; }

        /// <summary>
        /// 이벤트 진행으로 인해 수정된 시작 카드 뽑기권 개수. 최종적으로 해당 개수로 시작 카드 뽑기권이 설정됨.
        /// </summary>
        public int ModifiedTicket { get; set; }

        public void Init(int currentTicket, int newTicket)
        {
            OldTicket = currentTicket;
            NewTicket = newTicket;
            ModifiedTicket = newTicket;
        }

        public override void Clear()
        {
            OldTicket = 0;
            NewTicket = 0;
            ModifiedTicket = 0;
        }
    }
}