using System;
using System.Collections.Generic;
using TMPro;

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Cardevil.Dungeon.UI
{
    public class DungeonNodeUIDataComponent : MonoBehaviour
    {
        public int nodeId;
        public int nodeFloor;
        public DungeonNodeTypes nodeType;
        public DungeonNodePreset nodePreset;
        
        public List<DungeonNodeUIDataComponent> nextNodes = new List<DungeonNodeUIDataComponent>();
        public List<DungeonNodeUIDataComponent> prevNodes = new List<DungeonNodeUIDataComponent>();
        
        [Header("Debug")]
        [SerializeField] public bool showDebugLines = true;
        private LineRenderer lineRenderer;
        
        private void Awake()
        {
            enabled = false;
        }

        [HideInInspector, SerializeField] private DungeonNodeTypes prevNodeType;
        public void OnValidate()
        {
            if (enabled)
            {
                // name = $"NodeUI_{nodeId}";
                // if(prevNodeType != nodeType)
                // {
                //     TextMeshProUGUI text = GetComponentInChildren<TextMeshProUGUI>();
                //     prevNodeType = nodeType;
                //     if (text != null)
                //     {
                //         text.text = $"{nodeType}";
                //     }
                // }

                foreach (DungeonNodeUIDataComponent nxt in nextNodes)
                {
                    if (nxt == null) continue;
                    if (nxt.prevNodes.Contains(this))
                    {
                        continue;
                    }
                    nxt.prevNodes.Add(this);
                }
                
                
                if (showDebugLines)
                {
                    if (lineRenderer == null)
                    {
                        lineRenderer = GetComponent<LineRenderer>();
                        if (lineRenderer == null)
                        {
                            lineRenderer = gameObject.AddComponent<LineRenderer>();
                            lineRenderer.startWidth = 1f;
                            lineRenderer.endWidth = 1f;
                            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
                            lineRenderer.positionCount = 0;
                            lineRenderer.useWorldSpace = true;
                            lineRenderer.loop = false;
                            lineRenderer.startColor = Color.green;
                            lineRenderer.endColor = Color.green;
                        }
                    }

                    List<Vector3> linePositions = new List<Vector3>();
                    foreach (var nextNode in nextNodes)
                    {
                        if (nextNode != null)
                        {
                            linePositions.Add(transform.position);
                            linePositions.Add(nextNode.transform.position);
                        }
                    }

                    lineRenderer.positionCount = linePositions.Count;
                    lineRenderer.SetPositions(linePositions.ToArray());
                }
                else
                {
                    if (lineRenderer != null)
                    {
                        DestroyImmediate(lineRenderer);
                        lineRenderer = null;
                    }
                }
                #if UNITY_EDITOR
                PrefabUtility.RecordPrefabInstancePropertyModifications(this);
                #endif
               
               
            }
        }
    }
}