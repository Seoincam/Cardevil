using UnityEngine;


namespace Cardevil.InGame.Enemy
{
    public class Gimmick_Test : IGimmick
    {
        public void Apply(Enemy enemy)
        {
            Debug.Log($"{enemy.name}");
        }
    }
}
