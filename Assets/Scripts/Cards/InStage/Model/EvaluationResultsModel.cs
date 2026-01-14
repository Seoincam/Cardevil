using Cardevil.Attributes;
using Cardevil.Cards.Data;
using Cardevil.Cards.Data.InStage;
using Cardevil.Cards.InStage.Model.ReadOnly;
using Cardevil.Core;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Cardevil.Cards.InStage.Model
{
    [Serializable]
    public sealed class EvaluationResultsModel : IReadOnlyEvaluationResultsModel, IClearable
    {
        // 일단 전투의 모든 평가 결과를 저장함.
        [SerializeField, VisibleOnly] private List<EvaluationResult> history = new();
            
        public IReadOnlyList<EvaluationResult> History => history;

        public EvaluationResult? CurrentResult => history.Count == 0 ? null : history[^1];
        
        public void Clear()
        {
            history.Clear();
        }

        public void AddResult(EvaluationResult evaluationResult) => history.Add(evaluationResult);
    }

    [Serializable]
    public readonly struct EvaluationResult
    {
        public readonly IReadOnlyList<CardData> Cards;
        public readonly int TotalDamage;
        public readonly HandRanking HandRanking;
        public readonly Vector2Int PlayerPosition;

        public EvaluationResult(
            IReadOnlyList<CardData> cards, 
            int totalDamage, 
            HandRanking handRanking, 
            Vector2Int playerPosition)
        {
            Cards = cards;
            TotalDamage = totalDamage;
            HandRanking = handRanking;
            PlayerPosition = playerPosition;
        }
    }
}