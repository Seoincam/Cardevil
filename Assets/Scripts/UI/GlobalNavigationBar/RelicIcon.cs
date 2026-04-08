using Cardevil.Core.Attributes;
using Cardevil.Gameplay.Relics.Core;
using Cardevil.UI.Components;
using UnityEngine;
using UnityEngine.UI;

namespace Cardevil.UI.GlobalNavigationBar
{
    [RequireComponent(typeof(ShowTooltipOnHover))]
    public class RelicIcon : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Image icon;
        [SerializeField] private ShowTooltipOnHover tooltipTrigger;
        
        [Header("Definition References")]
        [SerializeField, VisibleOnly] private RelicDefinition definition;

        public void Initialize(RelicDefinition def)
        {
            definition = def;
            icon.sprite = def.DisplayIcon;
            tooltipTrigger.TooltipData.Title = def.DisplayName;
            tooltipTrigger.TooltipData.Description = def.DisplayDescription;
        }
    }
}