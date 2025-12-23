using Cardevil.Cards.Data;
using Cardevil.Cards.Data.InStage;
using Cardevil.Cards.Data.Modifiers;
using Cardevil.Cards.Data.Spec;
using Cardevil.DebugConsole;
using Cardevil.Utils;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

namespace Cardevil.Cards.Visual
{
    public class CardVisualTestUI : UI_Popup
    {
        [SerializeField] private CardVisualController visualController;
        [SerializeField] private List<UnityEngine.UI.Button> buttons;
        
        private CardData _selection1;
        private CardData _selection2;
        private CardData _selection3;

        private void OnEnable()
        {
            // 데이터 생성
            var cardSpec = new CardSpec(CardKind.Attack, 3);
            cardSpec.AddModifier(new ColorModifier(CardColor.Blue));
            cardSpec.AddModifier(new SelectableNumberModifier());
            cardSpec.AddModifier(new SelectableNumberConfirmModifier(2));
            _selection1 = cardSpec.Build();
            
            cardSpec.AddModifier(new SelectableNumberModifier());
            _selection2 = cardSpec.Build();
            
            cardSpec.AddModifier(new SelectableNumberModifier());
            _selection3 = cardSpec.Build();
            
            // Visual 초기화
            visualController.Init(_selection1);
            
            // 버튼 초기화
            if (buttons.Count < 3)
            {
                LogEx.LogError("Buttons count must be at least 3");
                return;
            }
            
            buttons[0].onClick.AddListener((() => {UpdateVisualAsync(_selection1).Forget();}));
            buttons[1].onClick.AddListener((() => {UpdateVisualAsync(_selection2).Forget();}));
            buttons[2].onClick.AddListener((() => {UpdateVisualAsync(_selection3).Forget();}));
        }

        private async UniTaskVoid UpdateVisualAsync(CardData data)
        {
            foreach (var button in buttons)
                button.interactable = false;
            await visualController.UpdateData(data);
            foreach (var button in buttons)
                button.interactable = true;
        }

        [ConsoleCommand("openCardVisualTest", "Pop Up Card Visual Test UI")]
        public static void TurnOn()
        {
            Managers.UI.ShowPopUpUI<CardVisualTestUI>();
        }

        [ConsoleCommand("closeCardVisualTest", "Close Card Visual Test UI")]
        public static void TurnOff()
        {
            Managers.UI.ClosePopUpUI();
        }
    }
}