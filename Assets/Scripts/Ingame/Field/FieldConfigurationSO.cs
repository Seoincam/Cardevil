using UnityEngine;
using UnityEngine.Serialization;

namespace Cardevil.Ingame.Field
{
    [CreateAssetMenu(fileName = "TileConfiguration", menuName = "Configuration/TileConfiguration")]
    public class FieldConfigurationSO : ScriptableObject
    {
        [Header("Tile Settings")]
        // [SerializeField] private Vector2Int gridSize = new Vector2Int(3, 3);
        [SerializeField] private float tileSize = 1.0f; 
        
        [Header("Tile Visual")]
        [SerializeField] private Color defaultTileColor = Color.white; 
        [SerializeField] private Color defaultTileHighlightColor = Color.yellow;
        
        // public Vector2Int GridSize => gridSize;
        public float TileSize => tileSize;
        public Color DefaultTileColor => defaultTileColor;
        public Color DefaultTileHighlightColor => defaultTileHighlightColor;
        
    }
}