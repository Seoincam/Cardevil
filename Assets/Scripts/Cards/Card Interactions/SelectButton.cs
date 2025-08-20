using Cardevil.Utils.Directions;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SelectButton : MonoBehaviour
{
    private TextMeshProUGUI text;
    public int NumberValue { get; private set; }
    public Direction DirectionValue { get; private set; }

    private enum CardType { Number, Direction }
    private CardType cardType;

    public event Action<int> OnNumberSelected;
    public event Action<Direction> OnDirectionSelected;


    public void Init(Action<int> OnNumberSelected, Action<Direction> OnDirectionSelected)
    {
        this.OnNumberSelected = OnNumberSelected;
        this.OnDirectionSelected = OnDirectionSelected;

        text = GetComponentInChildren<TextMeshProUGUI>();
        GetComponent<Button>().onClick.AddListener(OnClicked);
    }

    public void SetValue(int numberValue)
    {
        NumberValue = numberValue;
        text.text = numberValue.ToString();
        cardType = CardType.Number;
        gameObject.SetActive(true);
        Debug.Log("숫자");
    }

    public void SetValue(Direction directionValue)
    {
        DirectionValue = directionValue;
        text.text = directionValue.ToString();
        cardType = CardType.Direction;
        gameObject.SetActive(true);
        Debug.Log("방향");
    }

    private void OnClicked()
    {
        if (cardType == CardType.Number)
            OnNumberSelected?.Invoke(NumberValue);
        else
            OnDirectionSelected?.Invoke(DirectionValue);
    }
}
