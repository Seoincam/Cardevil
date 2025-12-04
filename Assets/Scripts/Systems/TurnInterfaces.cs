using Cysharp.Threading.Tasks;

namespace Cardevil.Systems
{
    /// <summary>
    /// 카드 관련 전체 턴 흐름을 관리하는 인터페이스.
    /// 
    /// <para>
    /// TurnManager는 게임의 턴 단위를 제어,
    /// ITurnCardFlow는 그 중 카드 단계(리롤/손패 사용 등)의
    /// 진입/종료와 내부 로직을 관리.
    /// </para>
    /// 
    /// 내부적으론 각 입력 인터페이스(ITurnRerollInput, ITurnPlayerInput)를
    /// 통해 실제 로직을 제공.
    /// </summary>
    public interface ITurnCardFlow
    {
        ITurnRerollInput Reroll { get; }
        ITurnPlayerInput StageCards { get; }
        
        /// <summary>
        /// 리롤 단계 진입 시 호출.
        /// 해당 단계의 Presenter/View를 활성화, 모델 초기화 및 슬롯 구성 수행.
        /// </summary>
        /// <param name="maxHand">손패의 최대 개수</param>
        UniTask EnterRerollPhase(int maxHand);
        
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
    }
    
    /// <summary>
    /// 카드 리롤 단계에서 플레이어 입력을 처리하는 인터페이스.
    /// 
    /// <para>
    /// ITurnCardFlow.Reroll.Reroll() 형태로 호출.
    /// 리롤, 모델 갱신, 연출을 수행.
    /// </para>
    /// </summary>
    public interface ITurnRerollInput
    {
        /// <summary>
        /// 리롤을 수행.
        /// 모델 갱신 및 연출 포함.
        /// </summary>
        UniTask Reroll();
    }
    
    /// <summary>
    /// 카드 선택/사용(Hand) 단계에서 플레이어 입력을 처리하는 인터페이스.
    /// 
    /// <para>
    /// ITurnCardFlow.Hand.* 형태로 호출.
    /// 드로우, 선택, 사용, 모델 갱신, 연출 등을 수행. 
    /// </para>
    /// </summary>
    public interface ITurnPlayerInput
    {
        /// <summary>
        /// 덱에 카드가 없는지 여부를 반환.
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
    }

    /// <summary>
    /// 플레이어의 행동을 정의하는 인터페이스.
    /// TurnManager가 호출.
    /// </summary>
    public interface ITurnPlayer
    {
        bool IsDead { get; }
        UniTask TurnAttack();
        UniTask TurnMove();
        void PlayerGetDamage(float amount);
    }

    /// <summary>
    /// 적의 행동을 정의하는 인터페이스.
    /// TurnManager가 호출.
    /// </summary>
    public interface ITurnEnemy
    {
        bool IsDead { get; }
        UniTask TurnAttack();
        bool GetDamage(float damage);

        /// <summary>
        /// 공격들중에 공격까지 남은 턴이 0이 있는것이 있나 됐나 확인.
        /// </summary>
        bool CheckAttack();
    }
}