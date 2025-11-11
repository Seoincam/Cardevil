using Cardevil.DataStructure.Serializables;
using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Cardevil.Cards
{
    public class CardVisualBase : MonoBehaviour
    {
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

        [Serializable]
        public struct SelectionGroup
        {
            [SerializeField] private int count;
            
            [Space, SerializeField] private GameObject backgroundGroup;
            [SerializeField] private GameObject numberGroup;

            [Space, SerializeField] private SerializableDictionary<Position, Image> backgroundMap;
            [SerializeField] private SerializableDictionary<Position, Image> numberMap;
            
            public enum Position
            {
                Top, Middle, Bottom
            }
        }
    }
    
}