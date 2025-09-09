using UnityEngine;

using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Shop : UI_Popup
{
    public override void Init()
    {
        base.Init();


        Bind<Button>(typeof(Buttons));

        GetButton((int)Buttons.Close).gameObject.AddUIEvent(CloseButtonClicked);
    }

    enum Buttons
    {
        Close
    }

    private void Start()
    {
        Init();

    }

    void CloseButtonClicked(PointerEventData eventData)
    {

    }
}
