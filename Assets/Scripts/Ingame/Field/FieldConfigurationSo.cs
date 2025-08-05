using UnityEngine;

namespace Cardevil.Ingame.Field
{
    [CreateAssetMenu(fileName = "TileConfiguration", menuName = "Configuration/TileConfiguration")]
    public class FieldConfigurationSo : ScriptableObject
    {
        [Header("Tile Settings")]
        [SerializeField] private Vector2Int gridSize = new Vector2Int(3, 3);
        [SerializeField] private float tileSize = 1.0f;
        
        
        public Vector2Int GridSize => gridSize;
        public float TileSize => tileSize;
        
    }
}