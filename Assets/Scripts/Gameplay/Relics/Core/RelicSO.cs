using UnityEngine;

namespace Cardevil.Gameplay.Relics.Core
{
    /// <summary>
    /// Relic을 유니티 에셋으로 만들기 위한 단순 래퍼.
    /// </summary>
    [CreateAssetMenu(menuName = "Relic/RelicSO")]
    public class RelicSO : ScriptableObject
    {
        [SerializeField] private RelicDefinition data;
        
        public RelicDefinition Data => data;

        
#if UNITY_EDITOR
        [SerializeField] private DataSource dataSource;

        public DataSource Source => dataSource;

        public bool FromLocal => dataSource == DataSource.Local;
        public bool FromSheet => dataSource == DataSource.Sheet;
        public bool IsMissing => dataSource == DataSource.Missing;
        
        public enum DataSource
        {
            Local,
            Sheet,
            Missing
        }
        
        public void InitializeFromSheet(
            string id,
            RelicRarity rarity,
            string displayName, 
            string displayDescription,
            string commentForEditor)
        {
            data = new RelicDefinition(id, rarity, displayName, displayDescription, commentForEditor);
            dataSource = DataSource.Sheet;
        }

        public void InitializeFromLocal(string id, string displayName)
        {
            data = new RelicDefinition(id, displayName);
            dataSource = DataSource.Local;
        }
#endif
    }
}