using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ReinforceValues", menuName = "Scriptable Objects/ReinforceValues")]
public class ReinforceValues : ScriptableObject
{
    [Header("Number Damage Reinforcement")]
    public int maxNumberDamageLevel;
    public List<float> damageMultiplyValues;

    [Header("Number Select Reinforcement")]
    public int maxNumberSelectLevel;

    [Header("Direction Select Reinforcement")]
    public int maxDirectionSelectLeve;
}
