using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public class CardSortingGroupFixer : EditorWindow
{
    [MenuItem("Tools/Fix Card Sorting Groups")]
    public static void FixSortingGroups()
    {
        string[] cardPaths = {
            "Assets/Prefabs/NewCard_Dev/Interaction Card.prefab",
            "Assets/Prefabs/NewCard_Dev/HandBar Card.prefab"
        };

        foreach (var path in cardPaths)
        {
            GameObject cardPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            if (cardPrefab == null)
            {
                Debug.LogError("Card prefab not found at " + path);
                continue;
            }

            // 1. Add SortingGroup to root
            var sortingGroup = cardPrefab.GetComponent<SortingGroup>();
            if (sortingGroup == null)
            {
                sortingGroup = cardPrefab.AddComponent<SortingGroup>();
                sortingGroup.sortingLayerName = "Default";
                sortingGroup.sortingOrder = 0;
                Debug.Log($"Added SortingGroup to {cardPrefab.name}");
            }

            // 2. Add SpriteMask to Background (Collider)
            Transform bgTransform = cardPrefab.transform.Find("Shared Layout Root/Background (Collider)");
            if (bgTransform != null)
            {
                var mask = bgTransform.GetComponent<SpriteMask>();
                if (mask == null)
                {
                    mask = bgTransform.gameObject.AddComponent<SpriteMask>();
                    var bgSprite = bgTransform.GetComponent<SpriteRenderer>().sprite;
                    mask.sprite = bgSprite;
                    mask.isCustomRangeActive = false;
                    Debug.Log($"Added SpriteMask to {cardPrefab.name}'s Background (Collider)");
                }
            }
            
            PrefabUtility.SavePrefabAsset(cardPrefab);
        }

        // 3. Update Layouts to VisibleInsideMask
        string[] layoutPaths = {
            "Assets/Prefabs/NewCard_Dev/Card Single Layout Root.prefab",
            "Assets/Prefabs/NewCard_Dev/Card Dual Layout Root.prefab",
            "Assets/Prefabs/NewCard_Dev/Card Triple Layout Root.prefab"
        };

        foreach (var path in layoutPaths)
        {
            GameObject layoutPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (layoutPrefab != null)
            {
                var renderers = layoutPrefab.GetComponentsInChildren<SpriteRenderer>(true);
                foreach (var r in renderers)
                {
                    r.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
                }
                PrefabUtility.SavePrefabAsset(layoutPrefab);
                Debug.Log("Updated MaskInteraction for " + layoutPrefab.name);
            }
        }

        Debug.Log("Card Sorting Group Fix completed successfully!");
    }
}
