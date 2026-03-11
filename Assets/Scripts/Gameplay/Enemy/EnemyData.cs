using UnityEngine;

namespace Cardevil.Gameplay.Enemy
{
    [CreateAssetMenu(fileName = "New Enemy Data", menuName = "Cardevil/Enemy Data")]
    public class EnemyData : ScriptableObject
    {
        [Header("기본 스탯")]
        public string enemyName;
        public float maxHP = 100f;
        public float damage = 1f;
        public int attackCycle = 3;

        [Header("외형")]
        public GameObject enemyPrefab; // 해당 데이터를 사용할 Enemy 프리팹
    }
}
