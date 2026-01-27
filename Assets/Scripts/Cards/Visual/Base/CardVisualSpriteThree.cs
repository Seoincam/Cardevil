using Cardevil.Attributes;
using Cardevil.Cards.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Cardevil.Cards.Visual
{
    /// <summary>
    /// 값이 세개일 때의 카드 비주얼.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class CardVisualSpriteThree : MonoBehaviour, ISimpleCardVisual
    {
        [field: Header("Image References")] 
        [field: SerializeField, VisibleOnly(EditableIn.EditMode)]
        public Image InnerFrame { get; private set; }
        
        [field: SerializeField, VisibleOnly(EditableIn.EditMode)]
        public Image TopNumber { get; private set; }
        
        [field: SerializeField, VisibleOnly(EditableIn.EditMode)]
        public Image MiddleNumber { get; private set; }
        
        [field: SerializeField, VisibleOnly(EditableIn.EditMode)]
        public Image BottomNumber { get; private set; }
        
        [field: Header("Other References")]
        [field: SerializeField, VisibleOnly(EditableIn.EditMode)]
        public CanvasGroup CanvasGroup { get; private set; }

        private void Reset()
        {
            CanvasGroup = GetComponent<CanvasGroup>();
        }

        public void ChangeVisualInstant(CardData cardData)
        {
            var spriteSet = CardSpriteSetConfigurationUtil.Configure(cardData);
            Debug.Assert(spriteSet.Phase == VisualPhase.Three);

            InnerFrame.sprite = spriteSet.InnerFrame;
            TopNumber.sprite = spriteSet.MainSprites[0];
            MiddleNumber.sprite = spriteSet.MainSprites[1];
            BottomNumber.sprite = spriteSet.MainSprites[2];
        }
    }
}