using Cardevil.NewCard.Common.Core;
using TMPro;
using UnityEngine;

namespace Cardevil.NewCard.InStage.Score
{
    public class CardScoreView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI totalScoreText;
        [SerializeField] private TextMeshProUGUI handRankText;
        
        public void UpdateHandRank(HandRank handRank)
        {
            if (handRank == HandRank.None)
            {
                handRankText.text = string.Empty;
                totalScoreText.text = string.Empty;
                return;
            }
            
            handRankText.text = handRank.ToString();
            UpdateTotalScore(Random.Range(1, 10));
        }

        public void UpdateTotalScore(int totalScore)
        {
            totalScoreText.text = totalScore.ToString();
        }
        
        public void Clear()
        {
            UpdateHandRank(HandRank.None);
        }

    }
}