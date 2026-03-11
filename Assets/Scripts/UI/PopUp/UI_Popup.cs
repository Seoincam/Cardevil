using Cardevil.Core.Systems;

namespace Cardevil.UI.PopUp
{
    public class UI_Popup : UI_Base
    {
        // Start is called before the first frame update
        public override void Init()
        {
            Managers.UI.SetCanvas(gameObject, true);
            SetResolution();
        }

        public virtual void ClosePopUPUI()
        {
            Managers.UI.ClosePopUpUI(this);
        }
    }
}
