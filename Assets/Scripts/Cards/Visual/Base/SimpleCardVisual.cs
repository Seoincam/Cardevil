using Cardevil.Attributes;
using Cardevil.Cards.Data.InStage;
using Cardevil.Cards.Visual.Sprites;
using Cardevil.Cards.Visual.StateMachine;
using UnityEngine;
using UnityEngine.UI;

namespace Cardevil.Cards.Visual.Base
{
    /// <summary>
    /// 단순한 값만 표시하는 기본 카드 비주얼 컴포넌트.
    /// InnerFrame, PrimaryValue, SmallNumber로 구성.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class SimpleCardVisual : MonoBehaviour
    {
        [field: Header("Image References")]
        [field: SerializeField, VisibleOnly(EditableIn.EditMode)]
        public Image InnerFrame { get; protected set; }
        
        [field: SerializeField, VisibleOnly(EditableIn.EditMode)]
        public Image PrimaryValue { get; protected set; }
        
        [field: SerializeField, VisibleOnly(EditableIn.EditMode)]
        public Image SmallNumber { get; protected set; }
        
        [field: Header("Other References")]
        [field: SerializeField, VisibleOnly(EditableIn.EditMode)]
        public CanvasGroup CanvasGroup { get; protected set; }

        private void Reset()
        {
            CanvasGroup = GetComponent<CanvasGroup>();
        }

        public virtual void ChangeVisualInstant(CardData cardData)
        {
            var spriteSet = CardSpriteSetConfigurationUtil.Configure(cardData);
            Debug.Assert(spriteSet.Phase == VisualPhase.One);

            InnerFrame.sprite = spriteSet.InnerFrame;
            PrimaryValue.sprite = spriteSet.Primary;

            if (spriteSet.HasSmallNumber)
            {
                SmallNumber.sprite = spriteSet.SmallNumber;
                SmallNumber.gameObject.SetActive(true);
            }
            else
            {
                SmallNumber.gameObject.SetActive(false);
            }
        }
        
        // TODO:
        // 카드 데이터 없이 더 간편하게 임시 카드 만들기
        // - ChangeVisual(CardColor color, int number)
        // - ChangeVisual(Direction[] dir) 
    }
}