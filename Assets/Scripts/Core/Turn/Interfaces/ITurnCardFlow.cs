using Cardevil.Cards.InStage;
using Cysharp.Threading.Tasks;
using System.Threading;

namespace Cardevil.Core.Turn.Interfaces
{
    public interface IReadOnlyCardFlow
    {
        /// <summary>
        /// 가장 최근 카드 사용 결과.
        /// </summary>
        EvaluationResult? Result { get; }
        
        /// <summary>
        /// 모든 카드 사용 결과를 저장하는 모델.
        /// </summary>
        IReadOnlyEvaluationResultsModel ResultsModel { get; }
        
        /// <summary>
        /// 현재 플레이어 카드 정보를 저장하는 모델.
        /// </summary>
        IReadOnlyStageCardsModel CardsModel { get; }
    }
    
    public interface ITurnCardFlow : IReadOnlyCardFlow
    {
        /// <summary>
        /// 모든 카드를 다 썼는지 여부.
        /// </summary>
        bool IsNoCard { get; }
        
        /// <summary>
        /// 덱에서 카드를 드로우해 손패에 추가.
        /// </summary>
        UniTask DrawCard();
        
        /// <summary>
        /// 유저가 카드 사용을 완료할 때까지 대기.
        /// 유저 입력이 완료되면 제어권 반환.
        /// </summary>
        UniTask WaitUserInput();

        /// <summary>
        /// 선택한 모든 카드를 사용함.
        /// 이때 이동 카드는 즉발, 공격 카드는 머리 위에 쌓임.
        /// </summary>
        UniTask UseAllCardsAsync(CancellationToken cancellationToken);
        
        UniTask<EvaluationResult> EvaluateAsync(CancellationToken cancellationToken);
    }
}