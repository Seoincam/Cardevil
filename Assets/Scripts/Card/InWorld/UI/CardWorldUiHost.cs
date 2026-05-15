using Cardevil.Card.InWorld.UI.Selection;
using Cardevil.Card.InWorld.UI.Upgrade;
using Cardevil.Core.Utils;
using UnityEngine;

namespace Cardevil.Card.InWorld.UI
{
    public class CardWorldUiHost : MonoBehaviour
    {
        private const string PrefabPath = "UI/CardFlow/CardWorldUiHost";

        [field: SerializeField] public CardSelectionView SelectionView { get; private set; }
        [field: SerializeField] public CardUpgradeView UpgradeView { get; private set; }

        public static CardWorldUiHost Instantiate()
        {
            var go = AssetUtil.Instantiate(PrefabPath);
            if (!go)
            {
                Debug.LogError($"Failed to instantiate card UI host prefab: Resources/Prefabs/{PrefabPath}");
                return null;
            }

            var host = go.GetComponent<CardWorldUiHost>();
            if (!host)
            {
                Debug.LogError($"Card UI host prefab does not contain {nameof(CardWorldUiHost)}: {PrefabPath}");
                Object.Destroy(go);
                return null;
            }

            return host;
        }
    }
}
