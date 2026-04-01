namespace Cardevil.Gameplay.Enemy.Attack
{
    public interface IAttackVisualizer
    {
        void ShowAttackSign_Point(int x, int y);
        void ShowAttackSign_Horizontal(int line);
        void ShowAttackSign_Vertical(int line);
    }
}