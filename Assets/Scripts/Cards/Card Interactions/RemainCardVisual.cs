using Cardevil.Cards;
using System.Linq;
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

    public void Init(CardData data)
    {
        this.data = data;
        isRemaining = true;
        UpdateVisual();
    }

    public void UpdateVisual()
    {
        isRemaining = Managers.Card.handBar.StageCardsCtx.Deck
                        .Any(c => c.id == data.id);
        // 색 설정
        cardImage.color = isRemaining ? Color.white : new Color(.5f, .5f, .5f);

        // 이름 설정
        if (data.valueType == CardData.ValueType.Move)
        {
            var move = data.Move;
            transform.name = move.direction.ToString();

            string textString;
            if (data.selectType == CardData.SelectType.All)
            {
                if (!move.isSet)
                    textString = "All";
                else
                    textString = move.direction.ToString() + "*";
            }
            else
            {
                textString = move.direction.ToString();
            }
            text.text = textString;
            text.fontSize = 25;
        }

        else if (data.valueType == CardData.ValueType.Number)
        {
            var number = data.Number;
            transform.name = $"{number.color} {number.number}";
            var textString = number.number == 0 ? "*" : number.number.ToString();
            if (number.number != 0 && data.CanOpenSelection)
                textString += "*";
            text.text = textString;
            switch (number.color)
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
}
