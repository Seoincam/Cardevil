using UnityEditor;
using UnityEngine;
using Cardevil.Dungeon.Build;
using Cardevil.Dungeon.UI;
using System.Collections.Generic;

namespace Cardevil.Dungeon.Editor
{
    /// <summary>
    /// 던전 검증 및 빌드 헬퍼 에디터 윈도우
    /// </summary>
    public class DungeonValidateWindow : EditorWindow
    {
        private const string AUTO_VALIDATE_KEY = "DungeonValidateWindow_AutoValidate";
        
        private Vector2 scrollPosition;
        private List<DungeonBuildHelperUI> buildHelpers = new List<DungeonBuildHelperUI>();
        private bool[] foldouts;
        private bool[] graphFoldouts;
        private static bool autoValidateOnPlay;

        [MenuItem("Cardevil/Dungeon Validate Window")]
        public static void ShowWindow()
        {
            var window = GetWindow<DungeonValidateWindow>("Dungeon Validate");
            window.minSize = new Vector2(400, 300);
            window.Show();
        }

        [InitializeOnLoadMethod]
        private static void InitializeAutoValidate()
        {
            // EditorPrefs에서 설정 로드
            autoValidateOnPlay = EditorPrefs.GetBool(AUTO_VALIDATE_KEY, false);
            
            // Play 모드 변경 이벤트 구독
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            // Edit 모드에서 Play 모드로 전환하려고 할 때
            if (state == PlayModeStateChange.ExitingEditMode && autoValidateOnPlay)
            {
                Debug.Log("[DungeonValidate] Play 모드 진입 전 자동 검증 시작...");
                
                // 현재 씬에서 DungeonBuildHelperUI 찾기
                var dungeonUI = FindFirstObjectByType<DungeonUI>();
                if (dungeonUI != null)
                {
                    var helpers = dungeonUI.GetComponentsInChildren<DungeonBuildHelperUI>(true);
                    int processedCount = 0;
                    
                    foreach (var helper in helpers)
                    {
                        if (helper == null) continue;
                        
                        helper.ReconnectAllNodesByHierarchy();
                        helper.ResetNodeIds();
                        EditorUtility.SetDirty(helper);
                        processedCount++;
                    }
                    
                    if (processedCount > 0)
                    {
                        Debug.Log($"[DungeonValidate] {processedCount}개의 던전 자동 검증 완료");
                    }
                    else
                    {
                        Debug.LogWarning("[DungeonValidate] 처리할 던전을 찾지 못했습니다.");
                    }
                }
                else
                {
                    Debug.LogWarning("[DungeonValidate] DungeonUI를 찾을 수 없습니다. 자동 검증을 건너뜁니다.");
                }
            }
        }

        private void OnEnable()
        {
            // EditorPrefs에서 설정 로드
            autoValidateOnPlay = EditorPrefs.GetBool(AUTO_VALIDATE_KEY, false);
            RefreshDungeonList();
        }

        private void RefreshDungeonList()
        {
            buildHelpers.Clear();
            
            // 씬에서 모든 DungeonBuildHelperUI 찾기
            var dungeonUI = FindFirstObjectByType<DungeonUI>();
            if (dungeonUI != null)
            {
                buildHelpers.AddRange(dungeonUI.GetComponentsInChildren<DungeonBuildHelperUI>(true));
            }
            
            // foldout 배열 초기화
            if (foldouts == null || foldouts.Length != buildHelpers.Count)
            {
                foldouts = new bool[buildHelpers.Count];
            }
            if (graphFoldouts == null || graphFoldouts.Length != buildHelpers.Count)
            {
                graphFoldouts = new bool[buildHelpers.Count];
            }
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("던전 검증 및 빌드 도구", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            // 자동 검증 설정
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("자동 검증 설정", EditorStyles.boldLabel);
            
            EditorGUI.BeginChangeCheck();
            bool newAutoValidate = EditorGUILayout.ToggleLeft(
                "Play 모드 진입 전 자동으로 던전 검증 수행", 
                autoValidateOnPlay
            );
            
            if (EditorGUI.EndChangeCheck())
            {
                autoValidateOnPlay = newAutoValidate;
                EditorPrefs.SetBool(AUTO_VALIDATE_KEY, autoValidateOnPlay);
                Debug.Log($"[DungeonValidate] 자동 검증 {(autoValidateOnPlay ? "활성화" : "비활성화")}");
            }
            
            if (autoValidateOnPlay)
            {
                EditorGUILayout.HelpBox(
                    "활성화됨: Edit 모드에서 Play 모드로 전환할 때 자동으로 모든 던전의 노드를 재연결하고 ID를 리셋합니다.",
                    MessageType.Info
                );
            }
            else
            {
                EditorGUILayout.HelpBox(
                    "비활성화됨: Play 모드 진입 전 자동 검증이 수행되지 않습니다.",
                    MessageType.Warning
                );
            }
            
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(5);

            // 새로고침 버튼
            if (GUILayout.Button("던전 리스트 새로고침", GUILayout.Height(30)))
            {
                RefreshDungeonList();
            }

            EditorGUILayout.Space(10);

            if (buildHelpers.Count == 0)
            {
                EditorGUILayout.HelpBox("씬에서 DungeonBuildHelperUI를 찾을 수 없습니다.", MessageType.Warning);
                return;
            }

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            for (int i = 0; i < buildHelpers.Count; i++)
            {
                var helper = buildHelpers[i];
                if (helper == null) continue;

                // 배열 길이 확인
                if (foldouts == null || i >= foldouts.Length || graphFoldouts == null || i >= graphFoldouts.Length)
                {
                    RefreshDungeonList();
                    if (foldouts == null || graphFoldouts == null) continue;
                }

                var chapterUI = helper.GetComponent<DungeonChapterUI>();
                string dungeonName = chapterUI != null 
                    ? $"Dungeon {chapterUI.DungeonId}" 
                    : $"Dungeon {i}";

                EditorGUILayout.BeginVertical("box");
                
                // Foldout 헤더
                foldouts[i] = EditorGUILayout.Foldout(foldouts[i], dungeonName, true, EditorStyles.foldoutHeader);

                if (foldouts[i])
                {
                    EditorGUI.indentLevel++;
                    
                    // 던전 정보
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("GameObject:", GUILayout.Width(100));
                    EditorGUILayout.ObjectField(helper.gameObject, typeof(GameObject), true);
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.Space(5);

                    // 노드 연결 버튼
                    if (GUILayout.Button("노드 연결 (Reconnect All Nodes By Hierarchy)", GUILayout.Height(25)))
                    {
                        Undo.RecordObject(helper, "Reconnect All Nodes");
                        helper.ReconnectAllNodesByHierarchy();
                        EditorUtility.SetDirty(helper);
                        Debug.Log($"[{dungeonName}] 노드 연결 완료");
                    }

                    // 노드 ID 리셋 버튼
                    if (GUILayout.Button("노드 ID 리셋 (Reset Node IDs)", GUILayout.Height(25)))
                    {
                        Undo.RecordObject(helper, "Reset Node IDs");
                        helper.ResetNodeIds();
                        EditorUtility.SetDirty(helper);
                        Debug.Log($"[{dungeonName}] 노드 ID 리셋 완료");
                    }

                    EditorGUILayout.Space(5);

                    // 전체 작업 버튼
                    GUI.backgroundColor = Color.green;
                    if (GUILayout.Button("전체 작업 실행 (Reconnect + Reset IDs)", GUILayout.Height(30)))
                    {
                        Undo.RecordObject(helper, "Full Dungeon Setup");
                        helper.ReconnectAllNodesByHierarchy();
                        helper.ResetNodeIds();
                        EditorUtility.SetDirty(helper);
                        Debug.Log($"[{dungeonName}] 전체 작업 완료");
                    }
                    GUI.backgroundColor = Color.white;

                    EditorGUILayout.Space(5);

                    // 추가 유틸리티 버튼들
                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button("연결 정보 출력"))
                    {
                        helper.PrintAllNodeConnections();
                    }
                    if (GUILayout.Button("노드 텍스트 설정"))
                    {
                        Undo.RecordObject(helper, "Set Node Texts");
                        helper.SetNodeTextsToType();
                        EditorUtility.SetDirty(helper);
                    }
                    if (GUILayout.Button("노드 텍스트 지우기"))
                    {
                        Undo.RecordObject(helper, "Clear Node Texts");
                        helper.ClearNodeTexts();
                        EditorUtility.SetDirty(helper);
                    }
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.Space(10);
                    
                    // 노드 그래프 시각화
                    EditorGUILayout.BeginVertical("box");
                    graphFoldouts[i] = EditorGUILayout.Foldout(graphFoldouts[i], "노드 그래프 (계층 구조)", true);
                    
                    if (graphFoldouts[i])
                    {
                        EditorGUILayout.Space(5);
                        
                        if (helper.rootNode != null)
                        {
                            DrawNodeGraphStack(helper.rootNode);
                        }
                        else
                        {
                            EditorGUILayout.HelpBox("루트 노드가 설정되지 않았습니다.", MessageType.Warning);
                        }
                    }
                    
                    EditorGUILayout.EndVertical();

                    EditorGUI.indentLevel--;
                }

                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(5);
            }

            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space(10);
            
            // 전체 던전 작업 버튼
            GUI.backgroundColor = Color.cyan;
            if (GUILayout.Button("모든 던전 전체 작업 실행", GUILayout.Height(35)))
            {
                if (EditorUtility.DisplayDialog("확인", 
                    "모든 던전에 대해 노드 연결 및 ID 리셋을 수행하시겠습니까?", 
                    "예", "아니오"))
                {
                    foreach (var helper in buildHelpers)
                    {
                        if (helper == null) continue;
                        
                        Undo.RecordObject(helper, "Full Dungeon Setup All");
                        helper.ReconnectAllNodesByHierarchy();
                        helper.ResetNodeIds();
                        EditorUtility.SetDirty(helper);
                    }
                    Debug.Log("모든 던전의 전체 작업이 완료되었습니다.");
                }
            }
            GUI.backgroundColor = Color.white;
        }
        
        // ... (기존 변수 및 초기화 코드는 그대로 유지) ...

        // [수정됨] 기존 DrawNodeGraphStack 대신 이 메서드와 헬퍼들을 사용하세요.
        
        /// <summary>
        /// 메인 노드 그래프 그리기 진입점
        /// </summary>
        private void DrawNodeGraphStack(DungeonNodeUIDataComponent rootNode)
        {
            if (rootNode == null) return;
            
            // 무한 루프 방지용 방문 기록
            HashSet<DungeonNodeUIDataComponent> globalVisited = new HashSet<DungeonNodeUIDataComponent>();
            
            // 메인 루트 그리기 시작
            DrawRecursive(rootNode, 0, globalVisited, null);
        }

        /// <summary>
        /// 재귀적으로 노드를 그립니다. (분기 발생 시 합류 지점을 찾아 처리)
        /// </summary>
        /// <param name="node">현재 그릴 노드</param>
        /// <param name="indent">들여쓰기 레벨</param>
        /// <param name="visited">방문 기록</param>
        /// <param name="stopNode">그리기 중단 지점 (분기 내부에서 합류점 도달 시 멈춤)</param>
        private void DrawRecursive(DungeonNodeUIDataComponent node, int indent, HashSet<DungeonNodeUIDataComponent> visited, DungeonNodeUIDataComponent stopNode)
        {
            if (node == null) return;
            
            // 합류 지점(StopNode)에 도달했으면 그리기 중단 (분기 종료)
            if (stopNode != null && node == stopNode) return;

            // 이미 방문한 노드 체크 (루프 방지)
            if (visited.Contains(node))
            {
                DrawNodeLine(node, indent, "↺ (Loop)", Color.red);
                return;
            }
            visited.Add(node);

            // 다음 노드 정보 확인
            var nextNodes = node.nextNodes;
            bool hasNext = nextNodes != null && nextNodes.Count > 0;
            bool isBranching = hasNext && nextNodes.Count > 1;

            // --- 1. 현재 노드 그리기 ---
            string prefix;
            if (indent == 0 && stopNode == null) prefix = "● "; // 전체 시작
            else if (stopNode != null) // 분기 내부일 때
            {
                // 분기 내부 로직: 다음 노드가 StopNode라면 '마지막' 처리
                bool isLastInBranch = !hasNext || (hasNext && nextNodes[0] == stopNode);
                prefix = isLastInBranch ? "└ " : "● "; 
            }
            else // 메인 줄기일 때
            {
                prefix = hasNext ? "├─ " : "└─ ";
            }

            DrawNodeLine(node, indent, prefix, GetNodeTypeColor(node.nodeType));

            // --- 2. 다음 단계 진행 ---
            if (!hasNext) return;

            if (isBranching)
            {
                // A. 분기 발생 (Branch Split)
                
                // 1. 합류 지점(Merge Node) 찾기
                // 모든 분기가 결국 어디서 만나는지 계산합니다.
                DungeonNodeUIDataComponent mergeNode = FindMergeNode(nextNodes);

                // 2. 각 분기 그리기
                for (int i = 0; i < nextNodes.Count; i++)
                {
                    var branchStartNode = nextNodes[i];
                    
                    // 분기 헤더 표시 (┌─ 분기 N)
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space((indent + 1) * 20); // 부모보다 한 칸 더 들여쓰기
                    GUI.color = new Color(1f, 0.8f, 0.3f); // 주황색
                    GUILayout.Label($"┌─ 분기 {i + 1}", EditorStyles.boldLabel);
                    GUI.color = Color.white;
                    EditorGUILayout.EndHorizontal();

                    // 분기 내부 그리기 (stopNode를 mergeNode로 설정)
                    // 들여쓰기는 헤더보다 한 칸 더(indent + 2) 들어갑니다.
                    DrawRecursive(branchStartNode, indent + 2, visited, mergeNode);
                }

                // 3. 합류 지점부터 메인 줄기 다시 그리기 (Resume Main Line)
                if (mergeNode != null)
                {
                    // 합류 지점은 다시 원래의 들여쓰기(indent)로 돌아와서 그립니다.
                    // visited에서 제거하지 않음 (한 번만 그려야 하므로)
                    // 하지만 DrawRecursive 호출을 위해 임시로 visited 체크를 우회해야 할 수도 있으나,
                    // 로직상 mergeNode는 아직 방문 안 함(StopNode 때문에).
                    
                    DrawRecursive(mergeNode, indent, visited, stopNode);
                }
            }
            else
            {
                // B. 직선 진행 (Linear)
                // 단순히 다음 노드로 재귀 호출
                DrawRecursive(nextNodes[0], indent, visited, stopNode);
            }
        }

        /// <summary>
        /// 노드 한 줄을 그리는 UI 헬퍼
        /// </summary>
        private void DrawNodeLine(DungeonNodeUIDataComponent node, int indent, string prefix, Color color)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(indent * 20); // 들여쓰기

            string label = $"{prefix}[ID:{node.nodeId}] {node.nodeType}";
            
            GUI.color = color;
            if (GUILayout.Button(label, EditorStyles.label, GUILayout.ExpandWidth(false)))
            {
                Selection.activeGameObject = node.gameObject;
                EditorGUIUtility.PingObject(node.gameObject);
            }
            GUI.color = Color.white;

            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// 여러 분기가 공통으로 도달하는 첫 번째 합류 노드(Merge Node)를 찾습니다.
        /// </summary>
        private DungeonNodeUIDataComponent FindMergeNode(List<DungeonNodeUIDataComponent> branchStarts)
        {
            if (branchStarts == null || branchStarts.Count < 2) return null;

            // 첫 번째 분기의 모든 후손 노드를 수집 (HashSet)
            HashSet<DungeonNodeUIDataComponent> candidates = GetReachableNodes(branchStarts[0]);

            // 나머지 분기들의 후손 노드와 교집합(Intersect) 계산
            for (int i = 1; i < branchStarts.Count; i++)
            {
                HashSet<DungeonNodeUIDataComponent> currentBranchNodes = GetReachableNodes(branchStarts[i]);
                candidates.IntersectWith(currentBranchNodes);
            }

            // 교집합이 없다면 합류하지 않음
            if (candidates.Count == 0) return null;

            // 교집합 후보들 중 "가장 가까운" 노드를 찾아야 함.
            // BFS를 통해 가장 먼저 만나는 후보가 합류점입니다.
            Queue<DungeonNodeUIDataComponent> queue = new Queue<DungeonNodeUIDataComponent>();
            HashSet<DungeonNodeUIDataComponent> bfsVisited = new HashSet<DungeonNodeUIDataComponent>();
            
            // 모든 분기 시작점을 큐에 넣고 시작
            foreach (var start in branchStarts)
            {
                queue.Enqueue(start);
                bfsVisited.Add(start);
            }

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();

                // 현재 노드가 교집합 후보군에 있다면, 이것이 첫 번째 합류점(Merge Node)
                if (candidates.Contains(current))
                {
                    return current;
                }

                if (current.nextNodes != null)
                {
                    foreach (var next in current.nextNodes)
                    {
                        if (next != null && !bfsVisited.Contains(next))
                        {
                            bfsVisited.Add(next);
                            queue.Enqueue(next);
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// 특정 노드로부터 도달 가능한 모든 노드를 반환 (DFS)
        /// </summary>
        private HashSet<DungeonNodeUIDataComponent> GetReachableNodes(DungeonNodeUIDataComponent startNode)
        {
            HashSet<DungeonNodeUIDataComponent> results = new HashSet<DungeonNodeUIDataComponent>();
            Stack<DungeonNodeUIDataComponent> stack = new Stack<DungeonNodeUIDataComponent>();
            
            if (startNode != null) stack.Push(startNode);

            while (stack.Count > 0)
            {
                var current = stack.Pop();
                if (results.Contains(current)) continue;
                
                results.Add(current);

                if (current.nextNodes != null)
                {
                    foreach (var next in current.nextNodes)
                    {
                        if (next != null) stack.Push(next);
                    }
                }
            }
            return results;
        }
        
        /// <summary>
        /// 노드 타입에 따른 색상 반환
        /// </summary>
        private Color GetNodeTypeColor(DungeonNodeTypes nodeType)
        {
            return nodeType switch
            {
                DungeonNodeTypes.None => Color.grey,                     // 회색
                DungeonNodeTypes.Mob => new Color(1f, 0.5f, 0.5f),       // 연한 빨강
                DungeonNodeTypes.MiniBoss => new Color(1f, 0.3f, 0.3f),  // 빨강
                DungeonNodeTypes.FinalBoss => new Color(1f, 0f, 0f),     // 진한 빨강
                DungeonNodeTypes.Heal => new Color(0.5f, 1f, 1f),        // 하늘색
                DungeonNodeTypes.Random => new Color(0.8f, 0.8f, 0.8f),  // 회색
                DungeonNodeTypes.BlackMarket => new Color(1f, 0.8f, 0.3f),// 주황
                DungeonNodeTypes.Shop => new Color(1f, 1f, 0.5f),        // 노랑
                _ => Color.white
            };
        }
    }
}