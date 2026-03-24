using Cardevil.Core.Attributes;
using Cardevil.Core.Utils;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Cardevil.UI.Components
{
    public class ShowTooltipOnHover : UIBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField, VisibleOnly] private HoverTooltip _hoverTooltip;
        [SerializeField, Tooltip("이벤트 트리거가 할당되면 PointerHandler는 비활성화 됩니다.")] private EventTrigger _eventTrigger;

        [field: SerializeField] public TooltipData TooltipData { get; private set; }
        private RectTransform _rectTransform;

#if UNITY_EDITOR

        protected override void Reset()
        {
            base.Reset();
            _eventTrigger = GetComponent<EventTrigger>();
            _rectTransform = GetComponent<RectTransform>();
        }
#endif

        protected override void Awake()
        {
            base.Awake();
            _rectTransform = GetComponent<RectTransform>();
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            if (_eventTrigger == null)
            {
                return;
            }

            var enterEntry = GetOrCreateEntry(EventTriggerType.PointerEnter);
            enterEntry.callback.AddListener(OnEventTriggerEnter);

            var exitEntry = GetOrCreateEntry(EventTriggerType.PointerExit);
            exitEntry.callback.AddListener(OnEventTriggerExit);
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            if (_eventTrigger == null)
            {
                return;
            }

            var enterEntry = GetOrCreateEntry(EventTriggerType.PointerEnter);
            enterEntry.callback.RemoveListener(OnEventTriggerEnter);
            var exitEntry = GetOrCreateEntry(EventTriggerType.PointerExit);
            exitEntry.callback.RemoveListener(OnEventTriggerExit);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_eventTrigger == null)
            {
                ShowTooltip();
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (_eventTrigger == null)
            {
                HideTooltip();
            }
        }

        private void OnEventTriggerEnter(BaseEventData eventData)
        {
            ShowTooltip();
        }

        private void OnEventTriggerExit(BaseEventData eventData)
        {
            HideTooltip();
        }

        public void ShowTooltip()
        {
            if (_hoverTooltip != null)
            {
                _hoverTooltip.ShowTooltip(TooltipData, _rectTransform);
                return;
            }

            var tooltip = AssetUtil.Instantiate("UI/HoverTooltip");
            if (tooltip == null)
            {
                return;
            }

            _hoverTooltip = tooltip.GetComponent<HoverTooltip>();
            if (_hoverTooltip == null)
            {
                AssetUtil.Destroy(tooltip);
                return;
            }

            _hoverTooltip.transform.SetParent(transform.root, false);
            _hoverTooltip.ShowTooltip(TooltipData, _rectTransform);
        }

        public void HideTooltip()
        {
            if (_hoverTooltip != null)
            {
                AssetUtil.Destroy(_hoverTooltip.gameObject);
                _hoverTooltip = null;
            }
        }

        private EventTrigger.Entry GetOrCreateEntry(EventTriggerType eventType)
        {
            foreach (var entry in _eventTrigger.triggers)
            {
                if (entry.eventID == eventType)
                {
                    return entry;
                }
            }

            var newEntry = new EventTrigger.Entry { eventID = eventType, callback = new EventTrigger.TriggerEvent() };
            _eventTrigger.triggers.Add(newEntry);
            return newEntry;
        }
    }
}