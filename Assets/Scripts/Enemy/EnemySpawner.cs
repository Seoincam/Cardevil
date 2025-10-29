using UnityEngine;

namespace Cardevil.Enemy
{
    public class EnemySpawner : MonoBehaviour
    {
        void Start()
        {
            Managers.Game._enemySpawner = this;
            // enemy 생성
        }



    }
}
