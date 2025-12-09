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



        private void Awake()
        {
            enabled = false;
        }

        [ContextMenu("Print Next Nodes")]
        public void PrintNextNodes()
        {
            Debug.Log($"Node {nodeId} has {nextNodes.Count} next nodes:");
            for (int i = 0; i < nextNodes.Count; i++)
            {
                if (nextNodes[i] != null)
                {
                    Debug.Log($"  [{i}] Node ID: {nextNodes[i].nodeId}");
                }
                else
                {
                    Debug.LogWarning($"  [{i}] NULL");
                }
            }
        }
        

        public void OnValidate()
        {
            // Preset에서 NodeType 가져오기
            if (nodePreset != null)
            {
                nodeType = nodePreset.NodeType;
                var ui = GetComponent<DungeonNodeUI>();
                if (ui != null)
                {
                    nodePreset.DrawNodeUI(ui, NodeState.Locked);
                }
            }
            else
            {
                nodeType = DungeonNodeTypes.None;
            }
        }

#if UNITY_EDITOR
        [Header("기즈모 설정")]
        private bool showGizmos = true;
        private Color gizmoLineColor = Color.green;
        private float gizmoNodeSize = 3f;
        private float gizmoSelectedNodeSize = 5f;
        
        private void OnDrawGizmos()
        {
            if (!showGizmos) return;

            // 일반 상태: 화살표 없이 연결선만 그리기
            Gizmos.color = gizmoLineColor;
            foreach (var nextNode in nextNodes)
            {
                if (nextNode == null) continue;
                Gizmos.DrawLine(transform.position, nextNode.transform.position);
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (!showGizmos) return;


            // BuildHelper가 선택되어 있으면 개별 노드의 Selected Gizmo를 그리지 않음
            var selectedObjects = UnityEditor.Selection.objects;
            foreach (var obj in selectedObjects)
            {
                if (obj is GameObject go && go.GetComponent<Cardevil.Dungeon.Build.DungeonBuildHelperUI>() != null)
                {
                    return; // BuildHelper가 선택되어 있으면 개별 노드 선택 표시 안함
                }
            }


            // 선택된 노드를 노란색으로 강조
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, gizmoSelectedNodeSize);

            // 다음 노드들 초록색으로 표시
            foreach (var nextNode in nextNodes)
            {
                if (nextNode == null) continue;
                
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(nextNode.transform.position, gizmoNodeSize);
                
                // 강조된 연결선
                DrawConnectionLine(transform.position, nextNode.transform.position, Color.cyan, true);
            }

            // 이전 노드들 빨간색으로 표시
            var allNodes = Object.FindObjectsByType<DungeonNodeUIDataComponent>(FindObjectsSortMode.None);
            foreach (var node in allNodes)
            {
                if (node.nextNodes.Contains(this))
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawWireSphere(node.transform.position, gizmoNodeSize);
                    
                    // 이전 노드에서 현재 노드로의 연결선
                    DrawConnectionLine(node.transform.position, transform.position, Color.red, true);
                }
            }
        }

        private void DrawConnectionLine(Vector3 from, Vector3 to, Color color, bool thick = false)
        {
            Gizmos.color = color;
            
            if (thick)
            {
                // 굵은 선 효과
                for (int i = -1; i <= 1; i++)
                {
                    Vector3 offset = new Vector3(i * 0.3f, 0, 0);
                    Gizmos.DrawLine(from + offset, to + offset);
                }
            }
            else
            {
                Gizmos.DrawLine(from, to);
            }
        }
#endif
    }
}