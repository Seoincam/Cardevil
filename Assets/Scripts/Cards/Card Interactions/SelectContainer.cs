using Cardevil.Utils.Directions;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Cardevil.Cards.CardInteractinos
{
    public class SelectContainer : MonoBehaviour
    {
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


        public void Init(Card card)
        {
            index = 0;
            this.card = card;
            if (options == null) options = new();
            else options.Clear();
        }

        public void AddOption(int number)
        {
            (int, Direction) option = new(number, Direction.None);
            options.Add(option);
            var currentIndex = index;
            buttons[index].onClick.AddListener(() => OnButtonSelected(currentIndex));
            buttons[index].GetComponentInChildren<TextMeshProUGUI>().text = number.ToString();
            index++;
        }

        public void AddOption(Direction direction)
        {
            (int, Direction) option = new(0, direction);
            options.Add(option);
            var currentIndex = index;
            buttons[index].onClick.AddListener(() => OnButtonSelected(currentIndex));
            buttons[index].GetComponentInChildren<TextMeshProUGUI>().text = direction.ToString();
            index++;
        }

        public void OpenSelection()
        {
            for (int i = 0; i < 9; i++)
                buttons[i].gameObject.SetActive(i < index);

            SetObjectActive(true);
        }

        private void OnButtonSelected(int index)
        {
            if (options[index].Item1 != 0)
                card.data.Number.number = options[index].Item1;
            else
                card.data.Move.direction = options[index].Item2;

            card.OnSelectValueEndEvent?.Invoke(card);
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
                card.OnSelectValueEndEvent?.Invoke(card);
        }
    }
}