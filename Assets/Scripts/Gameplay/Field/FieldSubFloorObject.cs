using Cardevil.Core.Utils;
using UnityEngine;

namespace Cardevil.Gameplay.Field
{
    public class FieldSubFloorObject : MonoBehaviour
    {
        [field:SerializeField] public MeshFilter MeshFilter { get; private set; }
        [field:SerializeField] public MeshRenderer MeshRenderer { get; private set; }
        
        private void Reset()
        {
            MeshFilter = GetComponent<MeshFilter>();
            MeshRenderer = GetComponent<MeshRenderer>();
        }
        
        public void Init(Material material)
        {
            if (MeshRenderer == null)
            {
                LogEx.LogError("MeshRenderer is not assigned.");
                return;
            }
            MeshRenderer.material = material;
        }

        public void SetMesh(Mesh mesh)
        {
            MeshFilter.mesh = mesh;
        }
    }
}