namespace Cardevil.Events
{
    /// <summary>
    /// 플레이어의 체력 변화 이벤트 인자.
    /// </summary>
    public class PlayerHealthChangeArgs : EventArgs<PlayerHealthChangeArgs>
    {
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

    public class RemainingCardChangeArgs : EventArgs<RemainingCardChangeArgs>
    {
        /// <summary>
        /// 새로 카드를 뽑은 뒤 덱에 남은 카드 개수.
        /// </summary>
        public int RemainingCardCount { get; private set; }

        public void Init(int remainingCardCount)
        {
            RemainingCardCount = remainingCardCount;
        }

        public override void Clear()
        {
            RemainingCardCount = 0;
        }
    }
}