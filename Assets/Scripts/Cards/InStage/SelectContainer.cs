using Cardevil.Cards.InStage.Presenter;
using Cardevil.Cards.InStage.ReadOnlyModel;
using Cardevil.Utils.Directions;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RemainCardVisual : MonoBehaviour
{
    public CardData data;
    public bool isRemaining;
    
    [Header("Visual")]
    [SerializeField] Image cardImage;
    [SerializeField] TextMeshProUGUI text;
    [SerializeField] Transform shakeObject;
    private IReadOnlyStageCardsModel _model;

    public void Init(IReadOnlyStageCardsModel model, CardData data)
    {
        this.data = data;
        isRemaining = true;
        _model = model;
        UpdateVisual();
    }

    public void UpdateVisual()
    {
        if (_model == null)
        {
            Debug.LogError("[RemainCardVisual] _model is null");
            return;
        }
        if (_model.Deck == null)
        {
            Debug.LogError("[RemainCardVisual] _model.Deck is null");
            return;
        }
        if (data == null)
        {
            Debug.LogError("[RemainCardVisual] data is null");
            return;
        }
        
        isRemaining = _model.Deck
                        .Any(c => c.id == data.id);
        // 색 설정
        cardImage.color = isRemaining ? Color.white : new Color(.5f, .5f, .5f);

        // 이름 설정
        if (data.valueType == CardData.ValueType.Move)
        {
            var move = data.Move;
            transform.name = move.DirectionValue.ToString();

            string textString;
            if (data.selectType == CardData.SelectType.All)
            {
                if (!move.IsSet)
                    textString = "All";
                else
                    textString = move.DirectionValue.ToString() + "*";
            }
            else
            {
                textString = move.DirectionValue.ToString();
            }
            text.text = textString;
            text.fontSize = 25;
        }

        else if (data.valueType == CardData.ValueType.Number)
        {
            var number = data.Number;
            transform.name = $"{number.ColorValue} {number.NumberValue}";
            var textString = number.NumberValue == 0 ? "*" : number.NumberValue.ToString();
            if (number.NumberValue != 0 && data.CanOpenSelection)
                textString += "*";
            text.text = textString;
            switch (number.ColorValue)
            {
                case NumberData.CardColor.Green: text.color = new Color(.25f, .7f, .25f); break;
                case NumberData.CardColor.Blue: text.color = Color.blue; break;
                case NumberData.CardColor.Red: text.color = Color.red; break;
                default: break;
            }
        }

        else
            Debug.LogError("cardData가 어떤 타입도 아닙니다.");
    }
}{
    public class SelectContainer : MonoBehaviour
    {
        private static readonly int[] AllNumberValues = new int[] { 2, 3, 4, 5, 6, 7, 8, 9, 10 };
        private static readonly Direction[] AllDirectionValues = new Direction[] { Direction.Up, Direction.Down, Direction.Left, Direction.Right };

        [SerializeField] Button buttonPrefab;
        [SerializeField] Button backgroundButton;
        private readonly List<Button> buttons = new();

        private int index;
        private Card card;
        private List<(int, Direction)> options;

        void Awake()
        {
            for (int i = 0; i < 9; i++)
            {
                buttons.Add(Instantiate(buttonPrefab, parent: transform));
                gameObject.SetActive(false);
            }

            backgroundButton.onClick.AddListener(OnBackgroundClicked);
        }

        private void AddOption(int number)
        {
            (int, Direction) option = new(number, Direction.None);
            options.Add(option);
            var currentIndex = index;
            buttons[index].onClick.AddListener(() => OnButtonSelected(currentIndex));
            buttons[index].GetComponentInChildren<TextMeshProUGUI>().text = number.ToString();
            index++;
        }

        private void AddOption(Direction direction)
        {
            (int, Direction) option = new(0, direction);
            options.Add(option);
            var currentIndex = index;
            buttons[index].onClick.AddListener(() => OnButtonSelected(currentIndex));
            buttons[index].GetComponentInChildren<TextMeshProUGUI>().text = direction.ToString();
            index++;
        }

        public void OpenSelection(Card card)
        {
            this.card = card;
            transform.position = new Vector3(card.transform.position.x, card.transform.position.y + 300f);
            options = new();
            index = 0;

            if (card.Data.valueType == CardData.ValueType.Number)
            {
                switch (card.Data.selectType)
                {
                    case CardData.SelectType.Multiple:
                        AddOption(card.Data.DefaultNumber.NumberValue);
                        foreach (var number in card.Data.NumberOptions)
                            AddOption(number);
                        break;

                    case CardData.SelectType.All:
                        foreach (var number in AllNumberValues)
                            AddOption(number);
                        break;
                }
            }
            else if (card.Data.valueType == CardData.ValueType.Move)
            {
                switch (card.Data.selectType)
                {
                    case CardData.SelectType.Multiple:
                        AddOption(card.Data.DefaultMove.DirectionValue);
                        AddOption(card.Data.DefaultMove.DirectionValue.Opposite());
                        break;

                    case CardData.SelectType.All:
                        foreach (var direction in AllDirectionValues)
                            AddOption(direction);
                        break; 
                }
            }

            for (int i = 0; i < 9; i++)
                buttons[i].gameObject.SetActive(i < index);

            SetObjectActive(true);
        }

        private void OnButtonSelected(int index)
        {
            if (options[index].Item1 != 0)
                card.Data.SelectValue(options[index].Item1);
            else
                card.Data.SelectValue(options[index].Item2);

            card.ValueSelectionEnded?.Invoke(card);
            SetObjectActive(false);
        }


        private void OnBackgroundClicked()
        {
            SetObjectActive(false);
        }

        private void SetObjectActive(bool value)
        {
            backgroundButton.gameObject.SetActive(value);
            gameObject.SetActive(value);
            if (!value)
                card.ValueSelectionEnded?.Invoke(card);
        }
    }
}