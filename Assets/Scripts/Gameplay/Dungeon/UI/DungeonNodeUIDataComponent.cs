using Cardevil.Core.Attributes;
using Cardevil.Gameplay.Dungeon.Build;
using Cardevil.Gameplay.Dungeon.Node;
using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Cardevil.Gameplay.Dungeon.UI
{
    public class DungeonNodeUIDataComponent : MonoBehaviour
    {
        public int nodeId;
        public int nodeFloor;
        [VisibleOnly] public DungeonNodeTypes nodeType;
        public DungeonNodePreset nodePreset;
        public string roomId;
        public bool useAutoGenerateRoomId = true;

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

            AutoSetRoomId();
        }
        
        public void AutoSetRoomId(bool force = false)
        {
            if (force || useAutoGenerateRoomId)
            {
                DungeonChapterUI parentHelper = GetComponentInParent<DungeonChapterUI>();
                string generatedId;
                // switch (nodeType)
                string bossPrefix = nodeType switch
                {
                    DungeonNodeTypes.FinalBoss => "FBoss",
                    DungeonNodeTypes.MiniBoss => "MBoss",
                    _ => ""
                };
                if (parentHelper == null)
                {
                    generatedId = $"{bossPrefix}Node{nodeId}";
                }
                else
                {
                    generatedId = $"{bossPrefix}{parentHelper.DungeonId}.{nodeId}";
                }
                roomId = generatedId;
                #if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(this);
                #endif
            }
            
        }

#if UNITY_EDITOR
        [Header("기즈모 설정")]
        [NonSerialized] private bool showGizmos = true;
        [NonSerialized] private Color gizmoLineColor = Color.green;
        [NonSerialized] private float gizmoNodeSize = 1.2f;
        [NonSerialized] private float gizmoSelectedNodeSize = 1.5f;
        
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
            bool isThis = false;
            foreach (var obj in selectedObjects)
            {
                if (obj == this.gameObject)
                {
                    isThis = true;
                    break;
                }
                if (obj is GameObject go && go.GetComponent<DungeonBuildHelperUI>() != null)
                {
                    return;
                }
            }
            if (!isThis) return;


            // 선택된 노드를 노란색으로 강조
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, gizmoSelectedNodeSize);

            // 다음 노드들 초록색으로 표시
            foreach (var nextNode in nextNodes)
            {
                if (nextNode == null) continue;
                
                Gizmos.color = Color.cyan;
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
                // 굵은 선 효과는 큐브로

                Vector3 direction = (to - from).normalized;
                float distance = Vector3.Distance(from, to);
                float thickness = 0.2f;
                Vector3 center = (from + to) / 2;
                Quaternion rotation = Quaternion.LookRotation(direction);
                Vector3 scale = new Vector3(thickness, thickness, distance);
                Gizmos.matrix = Matrix4x4.TRS(center, rotation, scale);
                Gizmos.DrawCube(Vector3.zero, Vector3.one);
                Gizmos.matrix = Matrix4x4.identity;
            }
            else
            {
                Gizmos.DrawLine(from, to);
            }
        }
#endif
    }
}