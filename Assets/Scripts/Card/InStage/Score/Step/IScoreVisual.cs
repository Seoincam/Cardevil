using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Cardevil.Card.InStage.Score.Step
{
    public interface IScoreVisual
    {
        Vector3 WorldPosition { get; }

        UniTask PlayReactionAsync();
    }
}