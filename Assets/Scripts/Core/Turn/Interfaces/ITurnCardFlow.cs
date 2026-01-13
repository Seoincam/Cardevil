using Cardevil.Cards.InStage.Model;
using Cardevil.Cards.InStage.Model.ReadOnly;
using Cardevil.Events;
using Cysharp.Threading.Tasks;

namespace Cardevil.Core.Turn.Interfaces
{
    public interface IReadOnlyCardFlow
    {
        /// <summary>
        /// 가장 최근 카드 사용 결과.
        /// </summary>
        EvaluationResult Result { get; }
        
        /// <summary>
        /// 모든 카드 사용 결과를 저장하는 모델.
        /// </summary>
        IReadOnlyEvaluationResultsModel ResultsModel { get; }
        
        /// <summary>
        /// 현재 플레이어 카드 정보를 저장하는 모델.
        /// </summary>
        IReadOnlyCardsModel CardsModel { get; }
    }
    
    public interface ITurnCardFlow : IReadOnlyCardFlow
    {
        /// <summary>
        /// 리롤 단계 진입 시 호출.
        /// 해당 단계의 Presenter/View를 활성화, 모델 초기화 및 슬롯 구성 수행.
        /// </summary>
        /// <param name="maxHand">손패의 최대 개수</param>
        UniTask EnterRerollPhase(int maxHand);
        
        /// <summary>
        /// 리롤을 수행.
        /// 모델 갱신 및 연출 포함.
        /// </summary>
        UniTask Reroll();
        
        /// <summary>
        /// 리롤 단계 종료 시 호출.
        /// 리롤 UI 비활성화.
        /// </summary>
        UniTask ExitRerollPhase();
        
        /// <summary>
        /// 리롤 단계의 실제 종료 시 호출.
        /// Presenter/View의 리소스 해제 및 Clear.
        /// </summary>
        void DeactivateReroll();
        
        /// <summary>
        /// 카드 선택/사용(Hand) 단계 진입 시 호출.
        /// 해당 단계의 Presenter/View를 활성화, 슬롯 구성 수행.
        /// </summary>
        UniTask EnterHandPhase();
        
        /// <summary>
        /// 카드 선택/사용(Hand) 단계 종료 시 호출.
        /// Hand UI 비활성화
        /// </summary>
        /// <returns></returns>
        UniTask ExitHandPhase();
        
        /// <summary>
        /// Hand 단계의 실제 죵료 시 호출.
        /// Presenter/View의 리소스 해제 및 Clear.
        /// </summary>
        void DeactivateHandPhase();
        
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
        /// <see cref="CardDamageEvaluationArgs"/>를 플레이어의 카드 사용 기반으로 초기화하여 반환함.
        /// </summary>
        CardDamageEvaluationArgs GetCardDamageEvaluationArgs();
    }
}