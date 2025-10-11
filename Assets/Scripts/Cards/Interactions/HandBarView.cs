using Cardevil.Utils;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Cardevil.Cards.Interactions
{
    public class HandBarView: MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private Button useButton;
        [SerializeField] private Button discardButton;
        [SerializeField] private Button sortByNumberButton;
        [SerializeField] private Button sortByIconButton;

        [Header("Text")] 
        [SerializeField] private TextMeshProUGUI deckCountText;
        [SerializeField] private TextMeshProUGUI discardCountText;
        
        private List<RectTransform> slots = new();
        private HandBarViewState? _lastState; // 같은 값 재적용 방지

        private void Awake()
        {
            if (useButton == null) LogEx.LogError("useButton == null");
            if (discardButton == null) LogEx.LogError("discardButton == null");
            if (sortByNumberButton == null) LogEx.LogError("sortByNumberButton == null");
            if (sortByIconButton == null) LogEx.LogError("sortByIconButton == null");
            if (deckCountText == null) LogEx.LogError("deckCountText == null");
            if (discardCountText == null) LogEx.LogError("discardCountText == null");
        }
        
        /// <summary>
        /// HandBar Button들에 Listener을 등록합니다.
        /// </summary>
        public void InitButtons(UnityAction use, UnityAction discard, UnityAction sortByNumber, UnityAction sortByIcon)
        {
            useButton.onClick.AddListener(use);
            discardButton.onClick.AddListener(discard);
            sortByNumberButton.onClick.AddListener(sortByNumber);
            sortByIconButton.onClick.AddListener(sortByIcon);
        }

        /// <summary>
        /// HandBar UI의 상태를 업데이트합니다.
        /// </summary>
        public void UpdateUI(HandBarViewState state)
        {
            if (_lastState is { } prev)
            {
                if (prev.CanUse != state.CanUse) useButton.interactable = state.CanUse;
                if (prev.CanDiscard != state.CanDiscard) discardButton.interactable = state.CanDiscard;
                if (prev.CanSort != state.CanSort)
                {
                    sortByNumberButton.interactable = state.CanSort;
                    sortByIconButton.interactable = state.CanSort;
                }

                if (prev.RemainingCards != state.RemainingCards) deckCountText.text = $"{state.RemainingCards}/50";
                if (prev.RemainingDiscards != state.RemainingDiscards) discardCountText.text = $"{state.RemainingDiscards}";
            }
            else 
            {
                useButton.interactable = state.CanUse;
                discardButton.interactable = state.CanDiscard;
                sortByNumberButton.interactable = state.CanSort;
                sortByIconButton.interactable = state.CanSort;
                
                deckCountText.text = $"{state.RemainingCards}/50";
                discardCountText.text = $"{state.RemainingDiscards}";
            }
            _lastState = state;
        }
    }
}