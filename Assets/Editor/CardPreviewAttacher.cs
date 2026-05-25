using UnityEngine;
using UnityEditor;
using Cardevil.Card.EditorTools;

public class CardPreviewAttacher : EditorWindow
{
    [MenuItem("Tools/Attach Card Previews to UI Host")]
    public static void AttachPreviews()
    {
        string prefabPath = "Assets/Resources/Prefabs/UI/CardFlow/CardWorldUiHost.prefab";
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

        if (prefab == null)
        {
            Debug.LogError("Failed to load prefab at " + prefabPath);
            return;
        }

        string cardPath = "Assets/Prefabs/NewCard_Dev/Interaction Card.prefab";
        GameObject cardPrefabGo = AssetDatabase.LoadAssetAtPath<GameObject>(cardPath);
        var cardPrefab = cardPrefabGo != null ? cardPrefabGo.GetComponent<Cardevil.Card.Common.InteractionCard>() : null;

        int count = 0;
        Transform[] allTransforms = prefab.GetComponentsInChildren<Transform>(true);
        foreach (Transform t in allTransforms)
        {
            if (t.name.Contains("Anchor"))
            {
                var preview = t.GetComponent<CardAnchorPreview>();
                if (preview == null)
                {
                    preview = t.gameObject.AddComponent<CardAnchorPreview>();
                }
                
                if (cardPrefab != null && preview.cardPrefab == null)
                {
                    preview.cardPrefab = cardPrefab;
                }
                count++;
            }
        }

        if (count > 0)
        {
            PrefabUtility.SavePrefabAsset(prefab);
            Debug.Log($"Successfully attached {count} CardAnchorPreview components to anchors in CardWorldUiHost.");
        }
        else
        {
            Debug.Log("No anchors found or they already have the component.");
        }
    }
}
