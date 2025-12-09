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
            foldouts = new bool[buildHelpers.Count];
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
                    EditorGUILayout.EndHorizontal();

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
    }
}