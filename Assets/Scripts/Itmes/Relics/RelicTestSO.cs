using System.Collections.Generic;
using UnityEngine;

namespace Cardevil
{
    [CreateAssetMenu(fileName = "RelicTestSO", menuName = "ScriptableObjects/RelicTestSO")]
    public class RelicTestSO : ScriptableObject
    {
        public List<string> playerRelics = new();
    }
}
