using Cardevil.Cards;
using Cardevil.Cards.CardInteractinos;
using Cardevil.Utils.Directions;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class SelectContainer : MonoBehaviour
{
    [SerializeField] SelectButton buttonPrefab;
    [SerializeField] Button backgroundButton;
    private SelectButton[] buttons;
    private Card card;

    void Awake()
    {
        buttons = new SelectButton[9];
        for (int i = 0; i < 9; i++)
        {
            buttons[i] = Instantiate(buttonPrefab, parent: transform).GetComponent<SelectButton>();
            buttons[i].Init(OnNumberSelected, OnDirectionSelected);
            gameObject.SetActive(false);
        }

        backgroundButton.onClick.AddListener(OnBackgroundClicked);
    }

    public void SetContainer(Card card, HashSet<int> numbers, Vector3 position)
    {
        this.card = card;
        transform.position = position + Vector3.up * 300f;

        var index = 0;
        foreach (var number in numbers)
            buttons[index++].SetValue(number);

        for (int i = index; i < buttons.Count(); i++)
            buttons[i].gameObject.SetActive(false);

        SetObjectActive(true);
    }

    public void SetContainer(Card card, HashSet<Direction> directions, Vector3 position)
    {
        this.card = card;
        transform.position = position + Vector3.up * 300f;

        var index = 0;
        foreach (var direction in directions)
            buttons[index++].SetValue(direction);

        for (int i = index; i < buttons.Count(); i++)
            buttons[i].gameObject.SetActive(false);

        SetObjectActive(true);
    }

    private void OnNumberSelected(int number)
    {
        var numberCardData = card.data as NumberCardData;
        numberCardData.SelectValue(number);
        card.cardVisual.UpdateVisual();
        SetObjectActive(false);
    }

    private void OnDirectionSelected(Direction direction)
    {
        var directionCardData = card.data as DirectionCardData;
        directionCardData.SelectValue(direction);
        card.cardVisual.UpdateVisual();
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
            card.OnSelectEndEvent?.Invoke(card);
    }
}
