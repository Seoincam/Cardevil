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
        [SerializeField] private RectTransform textAreaRect;
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI handRankText;
        
        [Space]
        [SerializeField] private GameObject elementTextPrefab;
        
        [Header("Settings")] 
        [SerializeField] private float bottomY = -255f;
        [SerializeField] private float spacing = 50f;
        
        private readonly Dictionary<IScoreOperator, TextMeshProUGUI> operatorTexts = new();
        
        private readonly Dictionary<ScoreOperatorType, char> operatorMap = new()
        {
            { ScoreOperatorType.Plus, '+' }, 
            { ScoreOperatorType.Multiply, 'X' },
        };

        public void UpdateHandRank(HandRank handRank, float score)
        {
            if (handRank == HandRank.None)
            {
                ClearScore();
                return;
            }
            
            handRankText.text = handRank.ToString();
            scoreText.text = score.ToString("F1");
        }
        
        public void ClearScore()
        {
            handRankText.text = string.Empty;
            scoreText.text = string.Empty;
        }
        
        public void AddOperator(IScoreOperator scoreOperator)
        {
            var text = CreateText(scoreOperator);
            operatorTexts.Add(scoreOperator, text);
            ScaleAsync().Forget();
        }

        public async UniTask PlayAddOperator(IScoreOperator scoreOperator)
        {
            var text = CreateText(scoreOperator);
            operatorTexts.Add(scoreOperator, text);
            await ScaleAsync();
        }

        public async UniTask ApplyOperator(IScoreOperator scoreOperator, float previousScore, float currentScore)
        {
            var targetText = operatorTexts[scoreOperator];
            operatorTexts.Remove(scoreOperator);
            
            targetText.rectTransform.DOAnchorPosY(-spacing, 0.25f);

            foreach (var text in operatorTexts.Values)
            {
                var currentY = text.rectTransform.anchoredPosition.y;
                var targetY = currentY - spacing;
                text.rectTransform.DOAnchorPosY(targetY, 0.25f);
            }

            await ScaleAsync();
            await DOTween.To(
                getter: () => previousScore,
                setter: value => scoreText.text = value.ToString("0"),
                endValue: currentScore,
                0.35f
            );
            await UniTask.Delay(TimeSpan.FromSeconds(0.1f));
        }
        
        private float GetY(int index)
        {
            return bottomY + index * spacing;  
        }

        private TextMeshProUGUI CreateText(IScoreOperator scoreOperator)
        {
            var operatorChar = operatorMap[scoreOperator.Type]; 
            var text = Instantiate(elementTextPrefab, textAreaRect).GetComponent<TextMeshProUGUI>();
            text.text = $"{operatorChar}{scoreOperator.Value:F1}";
            
            text.rectTransform.localEulerAngles = new Vector3(0f, 0f, Random.Range(-5f, 5f));
            
            text.rectTransform.anchoredPosition = new Vector2(120f, GetY(operatorTexts.Count));
            text.rectTransform.DOAnchorPosX(30f, .15f);

            return text;
        }

        private async UniTask ScaleAsync()
        {
            var count = operatorTexts.Count;
            
            textAreaRect.DOKill();
            
            if (count < 7)
            {
                await textAreaRect.DOScale(1f, 0.15f);
            }
            else
            {
                float targetScale = 7 / (float)count;
                await textAreaRect.DOScale(targetScale, 0.15f);
            }
        }
    }
}