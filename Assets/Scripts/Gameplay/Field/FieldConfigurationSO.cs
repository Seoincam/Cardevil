using Cardevil.Core.Utils;
using UnityEngine;

namespace Cardevil.Gameplay.Field
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
        [field:SerializeField] public Material[] Materials { get; private set; }
        [field:SerializeField] public Mesh[] Meshes { get; private set; }
        [field:SerializeField] public Mesh[] DoughnutMeshes { get; private set; }
        [field:SerializeField] public Mesh[] MunchkinMeshes { get; private set; }
            
        // public Vector2Int GridSize => gridSize;
        public float TileSize => tileSize;
        public Color DefaultTileColor => defaultTileColor;
        public Color DefaultTileHighlightColor => defaultTileHighlightColor;
        
        public Mesh GetMesh(int zeroIndexFloor)
        {
            if (zeroIndexFloor < 0 || zeroIndexFloor >= Meshes.Length)
            {
                LogEx.LogError($"Invalid floor index {zeroIndexFloor}. Returning default mesh.");
                return Meshes[0]; // 기본 메쉬 반환
            }
            return Meshes[zeroIndexFloor];
        }
        
        public Mesh GetDoughnutMesh(int zeroIndexFloor)
        {
            if (zeroIndexFloor < 0 || zeroIndexFloor >= Meshes.Length)
            {
                LogEx.LogError($"Invalid floor index {zeroIndexFloor}. Returning default mesh.");
                return DoughnutMeshes[0]; // 기본 메쉬 반환
            }
            return DoughnutMeshes[zeroIndexFloor];
        }
        public Mesh GetMunchkinMesh(int zeroIndexFloor)
        {
            if (zeroIndexFloor < 0 || zeroIndexFloor >= Meshes.Length)
            {
                LogEx.LogError($"Invalid floor index {zeroIndexFloor}. Returning default mesh.");
                return MunchkinMeshes[0]; // 기본 메쉬 반환
            }
            return MunchkinMeshes[zeroIndexFloor];
        }
        
        public Material GetMaterial(int zeroIndexStage)
        {
            if (zeroIndexStage < 0 || zeroIndexStage >= Materials.Length)
            {
                LogEx.LogWarning($"Invalid stage index {zeroIndexStage}. Returning default material.");
                return Materials[0]; // 기본 머티리얼 반환
            }
            return Materials[zeroIndexStage];
        }
    }
}