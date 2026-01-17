using Cardevil.Attributes;
using Cardevil.Cards.Data.InStage;
using Cardevil.Cards.Visual.StateMachine;
using Cardevil.DataStructure.Serializables;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Cardevil.Cards.Visual.New
{
    /// <summary>
    /// 선택 가능한 값이 2개 혹은 3개일 때 사용되는 카드 비주얼 컴포넌트.
    /// Phase에 따라 적절한 레이아웃으로 자동 전환함. 
    /// </summary>
    public class ChangeableCardVisual : CardVisual
    {
        [Header("Phase Groups")]
        [SerializeField, VisibleOnly(EditableIn.EditMode)]
        private SerializableDictionary<VisualPhase, PhaseGroup> selectionGroups;

        [Header("Backgrounds")] 
        [SerializeField, VisibleOnly(EditableIn.EditMode)]
        private SerializableDictionary<Position, RectTransform> selectionBackgrounds;
        
        public override void ChangeVisualInstant(CardData cardData)
        {
            var spriteSet = CardSpriteSetConfigurationUtil.Configure(cardData);

            switch (spriteSet.Phase)
            {
                case VisualPhase.One:
                    SetPhaseOne(spriteSet);
                    break;
                case VisualPhase.Two:
                    SetPhaseTwo(spriteSet);
                    break;
                case VisualPhase.Three:
                    SetPhaseThree(spriteSet);
                    break;
            }
        }

        private void SetPhaseOne(CardSpriteSet spriteSet)
        {
            Debug.Assert(spriteSet.Phase == VisualPhase.One);
            
            // 다른 Phase 비활성화
            SetInactivePhases(VisualPhase.Two, VisualPhase.Three);
            
            // Phase One 활성화
            InnerFrame.sprite = spriteSet.InnerFrame;
            SetSpriteAndActive(PrimaryValue, spriteSet.Primary);
            SetSpriteAndActive(SmallNumber, spriteSet.HasSmallNumber ? spriteSet.SmallNumber : null);
        }

        private void SetPhaseTwo(CardSpriteSet spriteSet)
        {
            Debug.Assert(spriteSet.Phase == VisualPhase.Two);
            Debug.Assert(spriteSet.MainSprites.Length == 2);
            
            SetInactivePhases(VisualPhase.One, VisualPhase.Three);
            
            // Phase Two 활성화 및 설정
            var group = selectionGroups[VisualPhase.Two];
            group.Group.SetActive(true);
            
            group.NumberMap[Position.Top].sprite = spriteSet.MainSprites[0];
            group.NumberMap[Position.Bottom].sprite = spriteSet.MainSprites[1];
            
            // 배경 위치 설정
            SetBackgroundActive(Position.Middle, BackgroundPositions.MiddleInit);
        }

        private void SetPhaseThree(CardSpriteSet spriteSet)
        {
            Debug.Assert(spriteSet.Phase == VisualPhase.Three);
            
            SetInactivePhases(VisualPhase.One, VisualPhase.Two);
            
            // Phase Three 활성화 및 설정
            var group = selectionGroups[VisualPhase.Three];
            group.NumberMap[Position.Top].sprite = spriteSet.MainSprites[0];
            group.NumberMap[Position.Middle].sprite = spriteSet.MainSprites[1];
            group.NumberMap[Position.Bottom].sprite = spriteSet.MainSprites[2];
            
            // 배경 위치 설정
            SetBackgroundActive(Position.Middle, BackgroundPositions.BottomFinal);
            SetBackgroundActive(Position.Bottom, BackgroundPositions.BottomFinal);
        }

        private void SetInactivePhases(params VisualPhase[] phases)
        {
            foreach (var phase in phases)
                SetInactivePhase(phase);
        }

        /// <summary>
        /// <see cref="phase"/>의 요소들을 비활성화함.
        /// </summary>
        private void SetInactivePhase(VisualPhase phase)
        {
            if (phase == VisualPhase.One)
            {
                PrimaryValue.gameObject.SetActive(false);
                SmallNumber.gameObject.SetActive(false);
                return;
            }
            
            if (selectionGroups.TryGetValue(phase, out var group))
                group.Group.SetActive(false);
            
            selectionBackgrounds[Position.Middle].gameObject.SetActive(false);
            selectionBackgrounds[Position.Bottom].gameObject.SetActive(false);
        }

        /// <summary>
        /// Image에 스프라이트를 설정하고 활성화. null이라면 비활성화.
        /// </summary>
        private static void SetSpriteAndActive(Image image, SpriteKey? spriteKey)
        {
            if (spriteKey is { IsValid: true })
            {
                image.sprite = spriteKey.Value;
                image.gameObject.SetActive(true);
            }
            else
            {
                image.gameObject.SetActive(false);
            }
        }
        
        /// <summary>
        /// 배경 RectTransform 위치 설정하고 활성화.
        /// </summary>
        private void SetBackgroundActive(Position position, Vector2 anchoredPosition)
        {
            var background = selectionBackgrounds[position];
            background.anchoredPosition = anchoredPosition;
            background.gameObject.SetActive(true);
        }
        
        /// <summary>
        /// Phase별 선택 가능한 숫자 그룹.
        /// </summary>
        [Serializable]
        public struct PhaseGroup
        {
            [field: SerializeField, VisibleOnly(EditableIn.EditMode)]
            public GameObject Group { get; private set; }

            [SerializeField, VisibleOnly(EditableIn.EditMode)]
            private SerializableDictionary<Position, Image> numberMap;
            
            public IReadOnlyDictionary<Position, Image> NumberMap => numberMap;
        }
        
        /// <summary>
        /// 선택 가능한 숫자의 위치.
        /// </summary>
        public enum Position
        {
            Top, Middle, Bottom
        }

        private static class BackgroundPositions
        {
            public static readonly Vector2 MiddleInit = new(160, -125);
            public static readonly Vector2 MiddleFinal = new(45, 0);
            public static readonly Vector2 BottomInit = new(80, -80);
            public static readonly Vector2 BottomFinal = new(0, 0);
        }
    }
}