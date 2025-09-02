using Cardevil.DataStructure;
using System;
using UnityEngine;

[CreateAssetMenu(fileName = "NumberReinforceConfiguration", menuName = "Cards/Number Damage Reinforce Configuration")]
public class NumberDamageReinforceConfiguration : ScriptableObject
{
    [Min(1), Tooltip("최대 강화 단계")]
    public int maxLevel;

    [Tooltip("레벨당 배율 (0레벨부터 시작)")]
    public float[] damageMultiply;

    [Tooltip("강화 가능성 [레벨: 성공 가능성]")]
    public SerializableDict<int, possiblity> possiblilities;

    [Serializable]
    public struct possiblity
    {
        public float success;
        public float retain;
        public float Fall => 1 - success - retain;
    }
}
