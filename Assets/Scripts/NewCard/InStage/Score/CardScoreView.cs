using Cardevil.NewCard.Common.Core;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Cardevil.NewCard.InStage.Score
{
    public class CardScoreView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI totalScoreText;
        [SerializeField] private TextMeshProUGUI handRankText;
        [SerializeField] private RectTransform scrollArea;
        
        [Space]
        [SerializeField] private GameObject elementTextPrefab;
        
        [Header("Settings")] 
        [SerializeField] private float bottomY = -255f;
        [SerializeField] private float spacing = 50f;
        [SerializeField] private AnimationType animationType = AnimationType.Scroll;
        
        private readonly Dictionary<IScoreOperator, TextMeshProUGUI> operatorTexts = new();
        
        private readonly Dictionary<ScoreOperatorType, char> operatorMap = new()
        {
            { ScoreOperatorType.Plus, '+' }, 
            { ScoreOperatorType.Multiply, 'X' },
        };
        
        private enum AnimationType
        {
            Scroll,
            Scale
        }
        
        public void UpdateHandRank(HandRank handRank, float score)
        {
            if (handRank == HandRank.None)
            {
                ClearScore();
                return;
            }
            
            handRankText.text = handRank.ToString();
            totalScoreText.text = score.ToString("F1");
        }

        public void UpdateTotalScore(int totalScore)
        {
            totalScoreText.text = totalScore.ToString();
        }
        
        public void ClearScore()
        {
            handRankText.text = string.Empty;
            totalScoreText.text = string.Empty;
        }
        
        public void AddOperator(IScoreOperator scoreOperator)
        {
            var text = CreateText(scoreOperator);
            operatorTexts.Add(scoreOperator, text);

            var count = operatorTexts.Count;
            switch (animationType)
            {
                case AnimationType.Scroll: 
                    ScrollAsync(count).Forget(); 
                    break;
                    
                case AnimationType.Scale:
                    ScaleAsync(count).Forget();
                    break;
            }
        }

        public async UniTask ApplyOperator(IScoreOperator scoreOperator, float previousScore, float currentScore)
        {
            var targetText = operatorTexts[scoreOperator];
            operatorTexts.Remove(scoreOperator);

            if (animationType == AnimationType.Scroll && scoreOperator.Index == 0)
            {
                var count = operatorTexts.Count;
                await ScrollAsync(0, count * 0.15f);
            }
            
            targetText.rectTransform.DOAnchorPosY(-spacing, 0.25f);

            foreach (var text in operatorTexts.Values)
            {
                var currentY = text.rectTransform.anchoredPosition.y;
                var targetY = currentY - spacing;
                text.rectTransform.DOAnchorPosY(targetY, 0.25f);
            }
            
            totalScoreText.text = currentScore.ToString("F1");

            if (animationType == AnimationType.Scale)
            {
                var count = operatorTexts.Count;
                ScaleAsync(count).Forget();
            }
            
            
            await UniTask.Delay(TimeSpan.FromSeconds(.25f));
        }

        private TextMeshProUGUI CreateText(IScoreOperator scoreOperator)
        {
            var operatorChar = operatorMap[scoreOperator.Type]; 
            var text = Instantiate(elementTextPrefab, scrollArea).GetComponent<TextMeshProUGUI>();
            text.text = $"{operatorChar}{scoreOperator.Value:F1}";
            
            text.rectTransform.localEulerAngles = new Vector3(0f, 0f, Random.Range(-5f, 5f));
            
            text.rectTransform.anchoredPosition = new Vector2(120f, GetY(operatorTexts.Count));
            text.rectTransform.DOAnchorPosX(30f, .15f);

            return text;
        }
        
        private float GetY(int index)
        {
            return bottomY + index * spacing;  
        }

        private async UniTask ScrollAsync(int currentIndex, float duration = 0.25f)
        {
            scrollArea.DOKill();

            if (currentIndex <= 7)
            {
                await scrollArea.DOAnchorPosY(0f, duration);
            }
            else
            {
                var targetY = (7 - currentIndex) * spacing;
                await scrollArea.DOAnchorPosY(targetY, duration);
            }
        }

        private async UniTask ScaleAsync(int currentIndex)
        {
            scrollArea.DOKill();
            
            if (currentIndex < 7)
            {
                await scrollArea.DOScale(1f, 0.25f);
            }
            else
            {
                float targetScale = 7 / (float)currentIndex * 0.9f;
                await scrollArea.DOScale(targetScale, 0.25f);
            }
        }
    }
}