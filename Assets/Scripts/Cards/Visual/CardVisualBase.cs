using Cardevil.Cards.ScriptableObjects;
using Cardevil.DataStructure.Serializables;
using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Cardevil.Cards.Visual
{
    public class CardVisualBase : MonoBehaviour
    {
        [Header("SO")] 
        [SerializeField] private CardSpriteAtlas spriteAtlas;
        [SerializeField] private CardVisualAnimSetting animSetting;
        
        [Header("Group")]
        [SerializeField] private RectTransform cardFrontGroup;
        [SerializeField] private RectTransform cardBackGroup;
        [SerializeField] private List<SelectionGroup> selectionGroups = new();

        [Header("Default")]
        [SerializeField] private Image innerFrame;
        [SerializeField] private Image number;

        [Header("Enhancement")] 
        [SerializeField] private List<Image> enhancementFrames;
        
        private bool _isFront = false;
        
        public RectTransform CardFrontGroup => cardFrontGroup;
        public RectTransform CardBackGroup => cardBackGroup;
        public IReadOnlyList<SelectionGroup> SelectionGroups => selectionGroups;
        
        public Image InnerFrame => innerFrame;
        public Image Number => number;
        
        public IReadOnlyList<Image> EnhancementFrames => enhancementFrames;
        
        public RectTransform Rect { get; private set; }

        private void Awake()
        {
            Rect = GetComponent<RectTransform>();
        }

        public void TryFlipFrontAnim(float duration, Ease ease = Ease.Unset)
        {
            if (_isFront) return;
            DOTween.Sequence()
                .Append(cardBackGroup.DOLocalRotate(new Vector3(0, 90, 0), duration * .5f).SetEase(ease))
                .Append(cardFrontGroup.DOLocalRotate(Vector3.zero, duration * .5f).SetEase(ease))
                .OnComplete(() => { _isFront = true; });
        }
        public void TryFlipFrontImmediate()
        {
            if (_isFront) return;
            FlipFront();
            _isFront = true;
        }

        public void TryFlipBackAnim(float duration, Ease ease = Ease.Unset)
        {
            if (!_isFront) return;
            DOTween.Sequence()
                .Append(cardFrontGroup.DOLocalRotate(new Vector3(0, 90, 0), duration * .5f).SetEase(ease))
                .Append(cardBackGroup.DOLocalRotate(Vector3.zero, duration * .5f).SetEase(ease))
                .OnComplete(() => {  _isFront = false; });
        }
        public void TryFlipBackImmediate()
        {
            if (!_isFront) return;
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

        
        [Serializable]
        public struct SelectionGroup
        {
            [SerializeField] private int count;
            
            [Space, SerializeField] private GameObject backgroundGroup;
            [SerializeField] private GameObject numberGroup;

            [Space, SerializeField] private SerializableDictionary<Position, Image> backgroundMap;
            [SerializeField] private SerializableDictionary<Position, Image> numberMap;
            
            // getter
            public int Count => count;
            
            public GameObject BackgroundGroup => backgroundGroup;
            public GameObject NumberGroup => numberGroup;
            
            public IReadOnlyDictionary<Position, Image> BackgroundMap => backgroundMap;
            public IReadOnlyDictionary<Position, Image> NumberMap => numberMap;
            
            public enum Position
            {
                Top, Middle, Bottom
            }
        }
    }

    public class CardVisualAnimSetting : ScriptableObject
    {
        public float numberScaleDur;
        public Ease numberScaleEase;
    }
    
}