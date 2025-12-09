using Cardevil.Cards.Visual.Base;
using Cardevil.DataStructure.Serializables;
using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Cardevil.Cards.Visual.Base
{
    public class CardVisualBase : CardVisualCore
    {
        [Header("SO")] 
        [SerializeField] private CardVisualAnimSetting animSetting;
        
        [Header("Group")]
        [SerializeField] private RectTransform cardFrontGroup;
        [SerializeField] private RectTransform cardBackGroup;
        [SerializeField] private List<SelectionGroup> selectionGroups = new();

        [Header("Background")]
        [SerializeField] private RectTransform selMidBackground;
        [SerializeField] private RectTransform selBotBackground;

        [Header("Enhancement")] 
        [SerializeField] private List<Image> enhancementFrames;
        
        [Serializable]
        public class SelectionGroup
        {
            [SerializeField] private int count;
            [SerializeField] private GameObject numberGroup;
            [SerializeField] private SerializableDictionary<Position, Image> numberMap;
            
            // getter
            public int Count => count;
            public GameObject NumberGroup => numberGroup;
            public IReadOnlyDictionary<Position, Image> NumberMap => numberMap;
        }
        
        public enum Position
        {
            Top, Middle, Bottom
        }
        
        private bool _isFront = true;
        
        public RectTransform SelMidBackground => selMidBackground;
        public RectTransform SelBotBackground => selBotBackground;
        
        public SelectionGroup GetSelectionGroup(int count)
        {
            foreach (var group in selectionGroups)
            {
                if (group.Count != count)
                    continue;
                return group;
            }

            return null;
        }

        public void TryFlipFrontAnim(float duration, Ease ease = Ease.Unset)
        {
            DOTween.Sequence()
                .Append(cardBackGroup.DOLocalRotate(new Vector3(0, 90, 0), duration * .5f).SetEase(ease))
                .Append(cardFrontGroup.DOLocalRotate(Vector3.zero, duration * .5f).SetEase(ease))
                .OnComplete(() => { _isFront = true; });
        }
        public void TryFlipFrontImmediate()
        {
            FlipFront();
            _isFront = true;
        }

        public void TryFlipBackAnim(float duration, Ease ease = Ease.Unset)
        {
            DOTween.Sequence()
                .Append(cardFrontGroup.DOLocalRotate(new Vector3(0, 90, 0), duration * .5f).SetEase(ease))
                .Append(cardBackGroup.DOLocalRotate(Vector3.zero, duration * .5f).SetEase(ease))
                .OnComplete(() => {  _isFront = false; });
        }
        public void TryFlipBackImmediate()
        {
            FlipBack();
            _isFront = false;
        }
        
        [ContextMenu("Flip Front")]
        private void FlipFront()
        {
            cardFrontGroup.eulerAngles = new Vector3(0, 0, 0);
            cardBackGroup.eulerAngles = new Vector3(0, 90, 0);
        }
        [ContextMenu("Flip Back")]
        private void FlipBack()
        {
            cardFrontGroup.eulerAngles = new Vector3(0, 90, 0);
            cardBackGroup.eulerAngles = new Vector3(0, 0, 0);
        }
        /*
         * 그냥 red2와 같은 걸 표시해야할 때는
         * CardData의 확장 메서드로 CreateCardData(red, 2)
         * 이런 식으로 만들 수 있게 해야겠음.
         * 값 선택, 강화창 등에서 쓸 수 있게.
         */
        
        public struct BackgroundPos
        {
            public static BackgroundPos MidSetting => new BackgroundPos(160, -125, 45, 0);
            public static BackgroundPos BotSetting => new BackgroundPos(80, -80, 0, 0);
            
            public Vector2 initPos;
            public Vector2 finalPos;

            private BackgroundPos(float initX, float initY, float finalX, float finalY)
            {
                initPos = new Vector2(initX, initY);
                finalPos = new Vector2(finalX, finalY);
            }
        }
    }

    public class CardVisualAnimSetting : ScriptableObject
    {
        public float numberScaleDur;
        public Ease numberScaleEase;
    }
    
}