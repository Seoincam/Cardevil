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
}