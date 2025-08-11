namespace Cardevil.Events
{
    public class PlayerHealthChangeArgs : EventArgs<PlayerHealthChangeArgs>
    {
        public int OldHealth { get; private set; }
        public int NewHealth { get; private set; }
        
        public void Init(int currentHealth, int maxHealth)
        {
            OldHealth = currentHealth;
            NewHealth = maxHealth;
        }
        public override void Clear()
        {
            
        }
    }
}