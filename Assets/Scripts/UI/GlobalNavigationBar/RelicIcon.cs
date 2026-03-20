using Cardevil.Gameplay.Relics.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Cardevil.UI.GlobalNavigationBar
{
    public class RelicIcon : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Image icon;
        
        [Header("Definition References")]
        [SerializeField] private RelicDefinition definition;

        public void Initialize(RelicDefinition def)
        {
            definition = def;
            icon.sprite = def.DisplayIcon;
        }
    }
}