using Cardevil.Core.Bootstrap;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Toolbars;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Cardevil.Core.Utils
{
    /// <summary>
    /// 메인 툴바에서 부트스트래퍼 씬을 통해 게임을 시작할 수 있는 버튼을 제공합니다.
    /// @machamy 코드.
    /// </summary>
    public static class MainToolbarStartBootstrapper
    {
        public const string BootstrapScenePath = "Assets/Scenes/BootstrapScene.unity";

        const string LogPrefix = "[MainToolbarStartBootstrapper]";

        // SessionState는 에디터 인스턴스별로 독립적으로 유지됩니다

        const string EditorSessionCountKey = "MainToolbarStartBootstrapper.SessionCount";

        [MainToolbarElement("Play/StartSceneFromBootstrapper", defaultDockPosition = MainToolbarDockPosition.Right)]
        public static MainToolbarButton StartSceneFromBootstrapperButton()
        {
            string text;
            Texture2D icon;
            if (Application.isPlaying)
            {
                text = "현재 플레이중...";
                icon = EditorGUIUtility.IconContent("sv_icon_dot12_pix16_gizmo").image as Texture2D;
            }
            else
            {
                text = "부트스트랩 하기";
                icon = EditorGUIUtility.IconContent("sv_icon_dot11_pix16_gizmo").image as Texture2D;
            }

            var content = new MainToolbarContent(text, icon, "부트스트래퍼 씬에서 게임을 시작합니다.");
            var button = new MainToolbarButton(content, OnButtonClick);

            return button;
        }

        private static void OnButtonClick()
        {
            if (Application.isPlaying)
            {
                Debug.LogWarning($"{LogPrefix} 이미 Play 모드입니다.");
                return;
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                Debug.Log($"{LogPrefix} 씬 저장이 취소되었습니다.");
                return;
            }

            EditorPrefs.DeleteKey(Bootstrapper.EditorStartSceneNameKey);
            EditorPrefs.DeleteKey(Bootstrapper.EditorStartScenePathKey);
            EditorPrefs.DeleteKey(EditorSessionCountKey);

            // 현재 씬 경로 저장
            string currentScenePath = SceneManager.GetActiveScene().path;
            string currentSceneName = SceneManager.GetActiveScene().name;

            // 현재 씬이 부트스트랩이면 무시
            if (currentScenePath == BootstrapScenePath)
            {
                Debug.LogWarning($"{LogPrefix} 현재 씬이 이미 부트스트래퍼 씬입니다. 시작만 합니다");
                EditorApplication.EnterPlaymode();
                return;
            }


            // Bootstrapper에도 전달
            EditorPrefs.SetString(Bootstrapper.EditorStartScenePathKey, currentScenePath);
            EditorPrefs.SetString(Bootstrapper.EditorStartSceneNameKey, currentSceneName);

            EditorSceneManager.OpenScene(BootstrapScenePath);

            EditorApplication.delayCall += () => { EditorApplication.EnterPlaymode(); };
        }

        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            MainToolbar.Refresh("Play/StartSceneFromBootstrapper");

            // SessionState 확인
            string previousScenePath = EditorPrefs.GetString(Bootstrapper.EditorStartScenePathKey, null);

            if (string.IsNullOrEmpty(previousScenePath))
            {
                return;
            }

            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                Debug.Log($"{LogPrefix} Play 모드 진입 - 부트스트래퍼 씬에서 게임 시작됨.");
                int sessionCount = EditorPrefs.GetInt(EditorSessionCountKey, 0);
                sessionCount++;
                EditorPrefs.SetInt(EditorSessionCountKey, sessionCount);
                Debug.Log(
                    $"{LogPrefix} {previousScenePath}({EditorPrefs.GetString(Bootstrapper.EditorStartSceneNameKey, "Unknown")}) 으로부터 시작됨. 세션 카운트: {sessionCount}");
            }

            // Play 모드가 완전히 종료된 후
            if (state == PlayModeStateChange.EnteredEditMode)
            {
                var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(previousScenePath);

                if (sceneAsset != null)
                {
                    EditorSceneManager.OpenScene(previousScenePath);
                    Debug.Log($"{LogPrefix} 이전 씬으로 복귀: {previousScenePath}");
                }

                // EditorPrefs도 정리
                int sessionCount = EditorPrefs.GetInt(EditorSessionCountKey, 0);
                sessionCount = Mathf.Max(0, sessionCount - 1);
                if (sessionCount <= 0)
                {
                    EditorPrefs.DeleteKey(Bootstrapper.EditorStartSceneNameKey);
                    EditorPrefs.DeleteKey(Bootstrapper.EditorStartScenePathKey);
                    Debug.Log($"{LogPrefix} 모든 세션 종료 - 이전 씬 정보 삭제");
                }
                else
                {
                    EditorPrefs.SetInt(EditorSessionCountKey, sessionCount);
                }
            }
        }
    }
}