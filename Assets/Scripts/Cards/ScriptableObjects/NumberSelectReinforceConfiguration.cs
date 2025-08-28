using UnityEngine;
using System;
using Cardevil.DataStructure;

[CreateAssetMenu(fileName = "NumberSelectReinforceConfiguration", menuName = "Cards/Number Select Reinforce Configuration")]
public class NumberSelectReinforceConfiguration : ScriptableObject
{

    [Min(1), Tooltip("최대 강화 단계")]
    public int maxLevel;

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
