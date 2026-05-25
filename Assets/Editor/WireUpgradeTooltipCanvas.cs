#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Cardevil.Card.EditorTools
{
    /// <summary>
    /// CardWorldUiHost.prefab 의 CardUpgradeView 컴포넌트에
    /// controlsCanvas 참조를 자동으로 연결하는 일회성 에디터 도구.
    /// </summary>
    public static class WireUpgradeTooltipCanvas
    {
        [MenuItem("Tools/Wire Upgrade Tooltip Canvas")]
        public static void Run()
        {
            const string prefabPath = "Assets/Resources/Prefabs/UI/CardFlow/CardWorldUiHost.prefab";
            var prefab = PrefabUtility.LoadPrefabContents(prefabPath);
            if (prefab == null)
            {
                Debug.LogError("[WireUpgradeTooltipCanvas] prefab not found: " + prefabPath);
                return;
            }

            // CardUpgradeView 컴포넌트 찾기
            var upgradeView = prefab.GetComponentInChildren<Card.InWorld.UI.Upgrade.CardUpgradeView>(true);
            if (upgradeView == null)
            {
                Debug.LogError("[WireUpgradeTooltipCanvas] CardUpgradeView not found in prefab.");
                PrefabUtility.UnloadPrefabContents(prefab);
                return;
            }

            // "Controls Canvas" 이름을 가진 Canvas 찾기
            Canvas targetCanvas = null;
            var canvases = prefab.GetComponentsInChildren<Canvas>(true);
            foreach (var c in canvases)
            {
                if (c.gameObject.name == "Controls Canvas")
                {
                    targetCanvas = c;
                    break;
                }
            }

            if (targetCanvas == null)
            {
                Debug.LogError("[WireUpgradeTooltipCanvas] 'Controls Canvas' not found in prefab.");
                PrefabUtility.UnloadPrefabContents(prefab);
                return;
            }

            // SerializedObject 로 controlsCanvas 필드 연결
            var so = new SerializedObject(upgradeView);
            var prop = so.FindProperty("controlsCanvas");
            if (prop == null)
            {
                Debug.LogError("[WireUpgradeTooltipCanvas] 'controlsCanvas' field not found on CardUpgradeView.");
                PrefabUtility.UnloadPrefabContents(prefab);
                return;
            }

            prop.objectReferenceValue = targetCanvas;
            so.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(prefab, prefabPath);
            PrefabUtility.UnloadPrefabContents(prefab);

            Debug.Log("[WireUpgradeTooltipCanvas] Successfully wired controlsCanvas → CardUpgradeView in " + prefabPath);
        }
    }
}
#endif
