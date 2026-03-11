using Cardevil.Core.Utils;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Cardevil.UI.PopUp
{
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
}
