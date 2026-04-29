using Cardevil.UI;
using Cardevil.UI.Components;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Cardevil.Gameplay.SpecialScenes
{
    public struct ShopEntryData
    {
        public int goldCost;
        public string TooltipKey;
    }
    
    
    [RequireComponent(typeof(ShowTooltipOnHover))]
    public class ShopItem : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private ShowTooltipOnHover _tooltip;
        [SerializeField] private Image _itemIcon;
        [SerializeField] private Image _costIcon;
        [SerializeField] private TMP_Text _costText;
        [SerializeField] private Button _button;
        
        
        public event Action OnItemClicked;

        private void Awake()
        {
            _tooltip = GetComponent<ShowTooltipOnHover>();
            if(_button == null)
                _button = GetComponent<Button>();
            if (_button != null)
            {
                _button.onClick.AddListener(HandleButtonClicked);
            }
        }

        public void Initialize(ShopEntryData data, TooltipData tooltipData)
        {
            _costText.text = data.goldCost.ToString();
            _tooltip.SetTooltipData(tooltipData);
        }
        
        private void HandleButtonClicked()
        {
            OnItemClicked?.Invoke();
        }
    }
}
