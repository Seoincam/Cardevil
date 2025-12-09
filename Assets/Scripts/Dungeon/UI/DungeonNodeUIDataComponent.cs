using Cardevil.Attributes;
using System.Collections.Generic;
using Cardevil.Dungeon;
using UnityEngine;
using UnityEngine.Serialization;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Cardevil.Dungeon.UI
{
    public class DungeonNodeUIDataComponent : MonoBehaviour
    {
        public int nodeId;
        public int nodeFloor;
        [VisibleOnly] public DungeonNodeTypes nodeType;
        public DungeonNodePreset nodePreset;

        public List<DungeonNodeUIDataComponent> nextNodes = new List<DungeonNodeUIDataComponent>();
        public List<DungeonNodeUIDataComponent> prevNodes = new List<DungeonNodeUIDataComponent>();

        // [Header("Debug")]
        // [SerializeField] public bool showDebugLines = true;
        // [SerializeField] private float debugLineWidth = 1f;
        // [SerializeField] private Color debugLineStartColor = Color.green;
        // [SerializeField] private Color debugLineEndColor = Color.green;
        // private LineRenderer lineRenderer;

        private void Awake()
        {
            enabled = false;
        }

        [ContextMenu("모든 라인렌더러 제거")]
        public void RemoveAllLineRenderers()
        {
            var DataComponents = GetComponents<DungeonNodeUIDataComponent>();
            foreach (var dataComponent in DataComponents)
            {
                var lineRenderers = dataComponent.GetComponents<LineRenderer>();
                foreach (var lr in lineRenderers)
                {
                    DestroyImmediate(lr);
                }
            }
        }

        [HideInInspector, SerializeField] private DungeonNodeTypes prevNodeType;

        public void OnValidate()
        {
            if (nodePreset != null)
            {
                nodeType = nodePreset.NodeType;
            }
            else
            {
                nodeType = DungeonNodeTypes.None;
            }
            
            if (enabled)
            {
                foreach (DungeonNodeUIDataComponent nxt in nextNodes)
                {
                    if (nxt == null) continue;
                    if (nxt.prevNodes.Contains(this))
                    {
                        continue;
                    }

                    nxt.prevNodes.Add(this);
                }
            }
        }
    }
}