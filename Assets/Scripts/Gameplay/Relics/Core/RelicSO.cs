using UnityEngine;

namespace Cardevil.Gameplay.Relics.Core
{
    /// <summary>
    /// Relic을 유니티 에셋으로 만들기 위한 단순 래퍼.
    /// </summary>
    [CreateAssetMenu(menuName = "Relic/RelicSO")]
    public class RelicSO : ScriptableObject
    {
        [SerializeField] private Relic data;

        public Relic Data => data;
        
#if UNITY_EDITOR
        public void Initialize(string id, string displayName)
        {
            data = new Relic(id, displayName);
        }
#endif
    }
}