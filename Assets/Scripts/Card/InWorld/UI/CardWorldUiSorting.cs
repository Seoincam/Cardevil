namespace Cardevil.Card.InWorld.UI
{
    public static class CardWorldUiSorting
    {
        public const string PopupSortingLayerName = "Card PopUp";
        public const int PopupSortingLayerID = -96533967;

        // All card-flow world sprites and canvases share this sorting layer.
        // Keep the order bands separated so Dim < Card sprites < UI < Header UI.
        public enum Order
        {
            Dim = 0,
            Card = 10000,
            Ui = 30000,
            CommonUi = 31000,
            Tooltip = 32000
        }
    }
}
