using Cardevil.Utils.Directions;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SelectContainer : MonoBehaviour
{
    [SerializeField] private Button[] buttons;

    public void SetContainer(HashSet<int> numbers, Vector3 position)
    {
        var myPosition = position + Vector3.up * 150f;
        transform.position = myPosition;

        var index = 0;
        foreach (var number in numbers)
            SetButton(index++, number.ToString());

        for (int i = index; i < buttons.Count(); i++)
            buttons[i].gameObject.SetActive(false);

        gameObject.SetActive(true);
    }

    public void SetContainer(HashSet<Direction> directions, Vector3 position)
    {
        var myPosition = position + Vector3.up * 150f;
        transform.position = myPosition;

        var index = 0;
        foreach (var direction in directions)
            SetButton(index++, direction.ToString());

        for (int i = index; i < buttons.Count(); i++)
            buttons[i].gameObject.SetActive(false);

        gameObject.SetActive(true);
    }

    private void SetButton(int index, string value)
    {
        // TODO: 성능 상 문제로 getcomponent 변경
        buttons[index].GetComponentInChildren<TextMeshProUGUI>().text = value;
        buttons[index].gameObject.SetActive(true);
    }
}
