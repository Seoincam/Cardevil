using Cardevil.Cards.Data;
using Cardevil.Cards.InStage.Model.ReadOnly;
using Cardevil.Cards.InStage.Presenter;
using Cardevil.Utils.Directions;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Cardevil.Cards.InStage
{
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
            /*
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
            */
        }

        private void OnButtonSelected(int index)
        {
            /*
            if (options[index].Item1 != 0)
                card.Data.SelectValue(options[index].Item1);
            else
                card.Data.SelectValue(options[index].Item2);

            card.ValueSelectionEnded?.Invoke(card);
            SetObjectActive(false);
            */
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