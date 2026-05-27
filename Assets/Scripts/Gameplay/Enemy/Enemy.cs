using Cardevil.Core.Events.EventArgs;
using Cardevil.Core.Events.ExecEvent;
using Cardevil.Core.Utils;
using Cardevil.Gameplay.Enemy.Attack;
using Cardevil.Gameplay.Enemy.Gimmick;
using Cardevil.Gameplay.Turn;
using Cardevil.Test.DebugConsole;
using Cardevil.Test.DebugConsole.Commands;
using Cysharp.Threading.Tasks;
using Database.Generated;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


namespace Cardevil.Gameplay.Enemy
{
    public enum AttackStyle
    {
        UnKnown, AttackPoint, AttackVertical, AttackHorizontal, HighCard, OnePair, TwoPair, Triple, Straight, Flush, FourCard, StraightFlush
    }

    public class Enemy : MonoBehaviour, IAttackVisualizer, ITurnEnemy
    {
        // ----- HP UI ----- ///
        [SerializeField] private Slider hpBar; // Inspector에서 UI Slider를 드래그하여 연결합니다.
        [SerializeField] private TMP_Text hpText;
        [SerializeField] private Image hpBarImage;
        [SerializeField] private Image hpBarGlowImage;

        public float maxHP = 100;
        public BaseMobBossData baseMobBossData;
        public Field.Field field;

        // ---- 기본 선언부 --- ///
        private float damage = 1; // Enemy의 공격력
        public float HP = 100; // Enemy의 체력
        public int attackCreateTurnOrder;
        public int attackCreateCycle = 3; // 일단 기본 3, 몇번마다 공격이 시행되는지?
        private bool aWakeFirst = true;
        private bool isEnemyDead = false;
        public bool isPlayerAttack = true;
        private bool orderSettingGo = false;
        private int settingOrder = 3;
        public int delayAttackByRelic = 0;
        public List<Attack.Attack> attackLists = new List<Attack.Attack>();
        public (bool attackSucess, float damage) _enemyAttackInfo;
        private AttackStyle currentAttackStyle;
        private List<AttackStyle> attackStyles = new List<AttackStyle>(); // Enemy에게 지정된 공격할 AttackStyle

        // -------- 족보 관련 ----------
        public int enforcedAttackRanking = 0;
        public int enforcedAttackDamage = 0;

        // -------- 기믹 관련 ----------
        private IGimmick gimmick;

        [Header("애니메이션 관련 연결")]
        [SerializeField] EnemyAnimationController _enemyAnimationController;
        [SerializeField] AttackCardAnimation _enemyAttackCardAnimation;

        public void Init(Field.Field field)
        {
            this.field = field;
            currentAttackStyle = AttackStyle.UnKnown;
            maxHP = HP; // 시작 시 HP를 최대 HP로 저장합니다.
        
        }

        #region ITurnEnemy 관련
        public bool IsDead => isEnemyDead || CurrentHp <= 0;

        public async UniTask OnEnemyCreateFirstAttackAsync(IEnemyContext context)
        {
            if (aWakeFirst)
            {
                LogEx.Log("Enemy 최초 턴! 첫 공격 패턴을 생성합니다.");
                // 적의 공격(SetupAttack)은 어차피 내부에서 Random.Range로 위치를 결정하므로,
                // 플레이어의 실제 위치가 없어도 임시 위치(0,0)를 넘겨 공격을 생성하고 하이라이트를 띄울 수 있습니다.
                AttackEnemyAwake(context.PlayerPosition());
                aWakeFirst = false;
            }
            await UniTask.CompletedTask;
        }

        public async UniTask OnStartTurnAsync()
        {
            // 턴 시작 시 상태 / 기믹 초기화
            EnemyTurnClear();
            LogEx.Log("EnemyTurn 시작!");
            await UniTask.CompletedTask;
        }

        public async UniTask OnTakeDamageAsync(float damage)
        {
            // 기존 피격 데미지 처리 로직
            GetDamage(damage);
            await UniTask.CompletedTask;
        }

        public async UniTask OnDieAsync()
        {
            // 기존 사망 처리 로직
            DeathAction();
            await UniTask.CompletedTask;
        }

        public async UniTask OnReplaceAsync()
        {
            // 교체 기믹이 아직 없으므로 로깅만 유지
            LogEx.LogWarning("OnReplaceAsync가 아직 구현되지 않음");
            await UniTask.CompletedTask;
        }

        public async UniTask<bool> CheckAttackCountAsync()
        {
            if (HP <= 0) return false;
            bool isAnyAttackReady = false;
            foreach (Attack.Attack attack in attackLists)
            {
                attack.attackTurnOrder--;
                LogEx.Log($"다음 공격까지 {attack.attackTurnOrder}턴 남았습니다.");
                if (attack.attackTurnOrder <= 0)
                {
                    isAnyAttackReady = true;
                }
            }
            return isAnyAttackReady;
        }

        public async UniTask<(bool success, int damage)> TryAttackAsync(IEnemyContext context)
        {
            bool isSuccess = false;
            float totalDamage = 0f;
            bool hasAttacked = false; // 공격을 한번이라도 시도했는지 체크

            foreach (Attack.Attack attack in attackLists)
            {
                if (attack.attackTurnOrder <= 0)
                {
                    // 애니메이션을 아직 안했다면, 데미지 판정 전에 먼저 돌진 애니메이션 실행!
                    if (!hasAttacked)
                    {
                        await _enemyAnimationController.EnemyAttackAnimation();
                        // 돌진이 끝나는 순간(타격 순간)에 다음 줄로 넘어갑니다.
                        hasAttacked = true;
                    }

                    // [핵심해결] CheckHit를 돌리기 전에, 플레이어의 최신 위치를 attack 객체에 반드시 갱신해야 합니다!
                    attack.playerPosition = context.PlayerPosition();
                    float damage = baseMobBossData.AttackDamage;

                    // HandRankAttackLogic에서 반환하는 out var resultInfo를 받아와서 처리합니다.
                    if (HandRankAttackLogic.CheckHit(attack, damage, out var resultInfo))
                    {
                        if (resultInfo.success)
                        {
                            _enemyAttackInfo.attackSucess = true;
                            isSuccess = true;
                            totalDamage += resultInfo.dmg; // resultInfo에 담긴 데미지를 합산
                        }
                    }
                    // 공격이 끝났으므로 해당 하이라이트 지우기
                    RemoveHighLight(attack);
                }
            }

            using (EnemyAttackAfterArgs args = EnemyAttackAfterArgs.Get())
            {
                ExecEventBus<EnemyAttackAfterArgs>.InvokeMerged(args).Forget();
            }

            return (isSuccess, (int)totalDamage);
        }

        public async UniTask OnAttackSuccessAsync()
        {
            // TryAttackAsync에서 success가 true일 때 턴 시스템이 호출해주는 메서드.
            // 공격이 성공했을 때 애니메이션이나 특별 기믹이 있다면 여기서 구현.
            // 혹은 공격이 성공했을 때 디버프 거는 무언가가 있으면 여기서 구현
            await UniTask.CompletedTask;
        }

        public async UniTask OnEndTurnAsync()
        {
            // 턴 종료 시 관련 기믹 처리 공간
            await UniTask.CompletedTask;
        }

        public async UniTask UpdateAttackAsync(IEnemyContext context)
        {
            TileVector playerPosition = context.PlayerPosition();

            // 턴 오더가 0이하가 되어 방금 사용된 공격들을 지우고 새로 생성(새 하이라이트 켜짐)
            int removedCount = attackLists.RemoveAll(attack => attack.attackTurnOrder <= 0);
            for (int i = 0; i < removedCount; i++)
            {
                CreateAttack(playerPosition);
            }

            if (attackLists.Count == 0 && !aWakeFirst)
            {
                CreateAttack(playerPosition);
            }

            if (orderSettingGo)
            {
                SetAllAttackOrderGo();
            }

            await UniTask.CompletedTask;
        }
        #endregion

        #region Attack 관련

        #region IAttackVisualizer 구현
        // HandRankAttackLogic에서 호출할 메서드들
        // 실제 구현되어있는 AttackNoticeSign 함수들과 연결
        public void ShowAttackSign_Point(int x, int y) { AttackNoticeSign_Point(x, y); }
        public void ShowAttackSign_Horizontal(int line) { AttackNoticeSign_Horizontal(line); }
        public void ShowAttackSign_Vertical(int line) { AttackNoticeSign_Vertical(line); }
        #endregion

        public virtual void AttackEnemyAwake(TileVector playerPosition) // 처음으로 호출되었을 때
        {
            if (aWakeFirst == true) // 처음에선 랜덤지정
            {
                CreateAttack(playerPosition, true);
            }
            aWakeFirst = false;
        }

        public void CreateAttack(TileVector playerPosition, bool firstCreate = false)
        {
            Attack.Attack tmpAttack = new() { playerPosition = playerPosition };
            tmpAttack.currentAttackStyle = SetAttackType(); // 무슨 공격인지 설정

            _enemyAttackCardAnimation.AttackAnimationStart(tmpAttack.currentAttackStyle).Forget();
            tmpAttack.SetAttackCycle(baseMobBossData.AttackCycle);
            Debug.Log($"적의 {tmpAttack.currentAttackStyle} 공격!");

            if (firstCreate)
            {
                tmpAttack.attackTurnOrder += delayAttackByRelic;
            }

            SetAttack(tmpAttack, baseMobBossData.AttackPlayer);
            attackLists.Add(tmpAttack); // 리스트에 어택 추가
        }

        public void AttackEnemyTurnStart(TileVector playerPosition)
        {
            EnemyTurnClear();
            LogEx.Log("EnemyTurn!!");
            AttackEnemyAwake(playerPosition); // Enemy Awake 시 실행되는 함수
                                              // 우선 시작할 때 공격구역 설정하도록 해보기

            // Enemy가 수행하는 공격들의 TurnOrder 감소 후 공격
            AttackPatternEnemyTurning();

            // Enemy 공격의 마무리 단계
            AttackEnemyTurnEnd(playerPosition);
        }

        public void AttackPatternEnemyTurning()
        {
            if (HP <= 0) // 만약 Enemy의 체력이 0이라면 공격을 수행하지 않음
            {
                return;
            }

            // 다른 Enemy들이 더 존재하는지 확인 후 다음 스테이지로
            foreach (Attack.Attack attack in attackLists) // 현재 Enemy가 가지고 있는 Attack
            {
                attack.attackTurnOrder--; // 모든 Attack들의 TurnOrder 감소
                LogEx.Log($"다음 공격까지 {attack.attackTurnOrder}턴 남았습니다 - {attack.currentAttackStyle} : {attack.attackLineNumber}");

                if (attack.attackTurnOrder <= 0) // 0이라면 공격 시행
                {
                    _enemyAnimationController.EnemyAttackAnimation().Forget();
                    AttackingCheck(attack);
                    RemoveHighLight(attack);
                }
            }
        }

        virtual public void AttackingCheck(Attack.Attack attack)
        {
            float damage = baseMobBossData.AttackDamage; // 데미지 수치가 있다면 가져오기

            // 로직 클래스에게 판정 위임
            if (HandRankAttackLogic.CheckHit(attack, damage, out var result))
            {
                LogEx.Log("Enemy가 공격에 성공했다!");
                _enemyAttackInfo.attackSucess = true;
            }
            else
            {
                _enemyAttackInfo.attackSucess = false;
                LogEx.Log("Enemy가 공격에 실패했다!");
            }

            // 공격 후 이벤트 처리
            using (EnemyAttackAfterArgs args = EnemyAttackAfterArgs.Get())
            {
                ExecEventBus<EnemyAttackAfterArgs>.InvokeMerged(args).Forget();
            }
        }

        void AttackEnemyTurnEnd(TileVector playerPosition)
        {
            // 공격이 끝난 (TurnOrder == 0) 항목들에 대해 하이라이트 해제 먼저 수행
            foreach (var attack in attackLists)
            {
                if (attack.attackTurnOrder <= 0) RemoveHighLight(attack);
            }

            // 리스트에서 조건에 맞는 요소 제거하고, 제거된 개수 반환 (RemoveAll)
            int removedCount = attackLists.RemoveAll(attack => attack.attackTurnOrder <= 0);

            // 제거된 개수만큼 새로운 공격 생성
            for (int i = 0; i < removedCount; i++)
            {
                LogEx.Log("지워진 Attack 만큼 새로 생성");
                CreateAttack(playerPosition);
            }

            if (orderSettingGo == true)
            {
                SetAllAttackOrderGo();
            }

            // 어택의 방식이 단일공격이라면 콤보라면, 이 지정이 아니라 특별하게 나오는 순서 같은게 지정될 수 있다.
            // 플레이어 턴으로 넘기기
        }

        /// <summary>
        /// true 라면 Player 위치를 받아와서 공격
        /// </summary>
        /// <param name="setPlayerAttack"></param>
        public virtual void SetAttack(Attack.Attack attack, bool setPlayerAttack = false)
        {
            attack.isPlayerAttack = setPlayerAttack;
            // 로직 클래스에게 설정 위임 (this는 IAttackVisualizer 구현체)
            HandRankAttackLogic.SetupAttack(attack, this);
        }

        int nowAttackPatternIndex = 0;
        AttackStyle SetAttackType()
        {
            AttackStyle returnAttackStyle;
            returnAttackStyle = baseMobBossData.AttackPattern[nowAttackPatternIndex++ % baseMobBossData.AttackPattern.Count];
            // 기믹으로 강화된 어택이 존재한다면
            returnAttackStyle += enforcedAttackRanking;
            return returnAttackStyle;
        }

        #endregion
        
        #region Player 상호작용

        public virtual bool GetDamage(float damage)
        {
            CurrentHp = HP - damage;
            LogEx.Log($"{damage}만큼의 피해를 입어 HP가 {CurrentHp}로 감소하였다!");
            UpdateHPBar();
            _enemyAnimationController.EnemyAttackedAnimation().Forget();
            return false; // 아직은 살아있다 추후에 사망처리
        }

        private bool DeathAction()
        {
            if (CurrentHp <= 0)
            {
                // 유닛 사망
                isEnemyDead = true;
                _enemyAnimationController.EnemyDeathAnimation().Forget();
                return true; // 사망시 true 변환
            }
            return false; // 아직 살아있다
        }

        #endregion

        #region Tool 관련
        private void EnemyTurnClear()
        {
            _enemyAttackInfo.attackSucess = false;
            orderSettingGo = false;
            settingOrder = attackCreateCycle;
        }

        private void UpdateHPBar()
        {
            if (hpBar != null)
            {
                // 현재 HP 값을 0과 1 사이의 값으로 정규화하여 Slider에 반영합니다.
                hpBar.value = HP / maxHP;
                hpText.text = $"{HP}  /  {maxHP}";
                if(baseMobBossData.MobType=="Boss")
                {
                    hpBarImage.sprite = Resources.Load<Sprite>("Arts/Ingame/HP_Bar/Main_Boss_HP/Main_Boss_Hp_Frame");
                    hpBarGlowImage.sprite = Resources.Load<Sprite>("Arts/Ingame/HP_Bar/Main_Boss_HP/Main_Boss_Hp_Glow");
                }
                else
                {
                    hpBarImage.sprite = Resources.Load<Sprite>("Arts/Ingame/HP_Bar/Middle_Boss_HP/Middle_Boss_HP_Frame");
                    hpBarGlowImage.sprite = Resources.Load<Sprite>("Arts/Ingame/HP_Bar/Middle_Boss_HP/Middle_Boss_HP_Glow");
                }
            }
        }

        public void SetAllAttackOrder(int i)
        {
            orderSettingGo = true;
            settingOrder = i;
        }

        public void SetAllAttackOrderGo()
        {
            int i = settingOrder;
            foreach (Attack.Attack attack in attackLists)
            {
                if (i <= attack.attackTurnOrder)
                {
                    attack.attackTurnOrder = i;
                    LogEx.Log($"attackTurnOrder이 {i}로 조정되었습니다.");
                }
                else
                {
                    LogEx.Log($"공격턴 오더 {i}가 현재 공격턴 오더 {attack.attackTurnOrder}보다 커서 조정되지 않았습니다");
                }
            }
        }

        public void AttackOrderDiscount()
        {
            foreach (Attack.Attack attack in attackLists)
            {
                if (attack.attackTurnOrder <= 0)
                {
                    attack.attackTurnOrder--;
                    LogEx.Log("공격 턴 추가 감소!");
                }
            }
        }

        /// <summary>
        /// Player 위치로 공격하게끔
        /// </summary>
        public void SetAttackOnPlayer()
        {
            isPlayerAttack = true;
        }

        /// <summary>
        /// true or false 랜덤 설정
        /// </summary>
        public void SetAttackOnPlayerOrRandom()
        {
            isPlayerAttack = (UnityEngine.Random.value > 0.5f);
        }

        public void SetFirstAwake()
        {
            aWakeFirst = true;
        }

        /// <summary>
        /// Helper: check bounds inside 3x3 field
        /// </summary>
        private bool IsValidPoint(int x, int y)
        {
            return (x >= 0 && x < 3 && y >= 0 && y < 3);
        }
        #endregion

        #region EnemySpawner 세팅
        public float CurrentHp
        {
            get => HP;
            set
            {
                using (EnemyHealthChangeArgs args = EnemyHealthChangeArgs.Get())
                {
                    args.Init(HP, value, this);
                    ExecEventBus<EnemyHealthChangeArgs>.InvokeMerged(args).Forget();
                    HP = args.ModifiedHealth;
                }
            }
        }

        /// <summary>
        /// Enemy 세팅 시작
        /// </summary>
        /// <param name="_baseMobBossData"></param>
        public void Setup(BaseMobBossData _baseMobBossData)
        {
            baseMobBossData = _baseMobBossData;
            Debug.Log($"SetUp! : {baseMobBossData.MobKorID} : {_baseMobBossData.MobID}");

            attackCreateCycle = _baseMobBossData.AttackCycle;

            if (baseMobBossData.BoolAttackType)
            {
                attackStyles = baseMobBossData.AttackPattern;
            }
            else
            {
                // 0이라면 가중치에 따라 랜덤
                attackStyles = Util.PickRandomPatterns(baseMobBossData.AttackPattern, baseMobBossData.AttackWeight, 15);
            }

            isPlayerAttack = baseMobBossData.AttackPlayer;

            // HP 설정
            maxHP = _baseMobBossData.BaseHP;
            CurrentHp = maxHP;

            _enemyAnimationController.EnemyStartAnimation(baseMobBossData.MobID).Forget();

            UpdateHPBar(); // 시작 시 HP바를 초기화합니다.

            SettingGimmick(_baseMobBossData);
        }

        private void SettingGimmick(BaseMobBossData baseMobBossData)
        {
            // TODO : 여러개 기믹이 존재하는 몹이 있으면 [0]을 수정해야함.
            char trimText = '"';
            IGimmick igimmick = GimmickFactory.Instance.CreateGimmick(baseMobBossData.GimmickName[0].ToString().Trim(trimText));
            gimmick = igimmick;

            if (igimmick == null)
            {
                return;
            }
            igimmick.Apply(this);
        }

        #endregion

        #region HighLight 관련
        // 리팩토링 RemoveHighLight
        public void RemoveHighLight(Attack.Attack attack)
        {
            if (attack == null) return;

            // 가로/세로 공격 처리
            if (attack.currentAttackStyle == AttackStyle.AttackHorizontal)
            {
                RemoveHighLight_Horizontal(attack.attackLineNumber);
                return;
            }
            if (attack.currentAttackStyle == AttackStyle.AttackVertical)
            {
                RemoveHighLight_Vertical(attack.attackLineNumber);
                return;
            }

            // 스트레이트 플러시 (예외케이스) 처리
            if (attack.currentAttackStyle == AttackStyle.StraightFlush)
            {
                RemoveHighLight_StraightFlush(attack);
                return;
            }

            // 나머지 점 (Point) 기반 공격들 통합 처리
            // 메인 포인트 해제
            RemoveHighLight_Point(attack.attackPointNumber_x, attack.attackPointNumber_y);

            // 추가 포인트들 해제 (null 체크 포함하여 한번에 처리)
            if (attack.attackPointNumberExtra_x != null)
            {
                for (int i = 0; i < attack.attackPointNumberExtra_x.Length; i++)
                {
                    RemoveHighLight_Point(attack.attackPointNumberExtra_x[i], attack.attackPointNumberExtra_y[i]);
                }
            }
        }

        public void RemoveHighLight_Horizontal(int lineNumber)
        {
            for (int x = 0; x < 3; x++) // 가로 0,1,2 에 대해
            {
                field[lineNumber][x].Unhightlight(Define.HighlightType.DefaultAttack); // 해당 타일을 하이라이트 하기.
            }
        }

        public void RemoveHighLight_Vertical(int lineNumber)
        {
            for (int x = 0; x < 3; x++) // 가로 0,1,2 에 대해
            {
                field[x][lineNumber].Unhightlight(Define.HighlightType.DefaultAttack); // 해당 타일을 하이라이트 하기.
            }
        }

        void RemoveHighLight_Point(int pointNumber_x, int pointNumber_y)
        {
            if (IsValidPoint(pointNumber_x, pointNumber_y))
                field[pointNumber_x][pointNumber_y].Unhightlight(Define.HighlightType.DefaultAttack); // 해당 타일 하이라이트 off
        }

        public void AttackNoticeSign_Point(int pointNumber_x, int pointNumber_y)
        {
            if (IsValidPoint(pointNumber_x, pointNumber_y))
            {
                field[pointNumber_x][pointNumber_y].Highlight(Define.HighlightType.DefaultAttack); // 해당 타일을 하이라이트 하기.
                currentAttackStyle = AttackStyle.AttackPoint; // 포인트 어택 형태로 저장
            }
        }

        // 스트레이트 플러시 전용 해제 로직 분리
        private void RemoveHighLight_StraightFlush(Attack.Attack attack)
        {
            // 정적 리스트로 관리하거나 상수로 빼는 것이 좋음 (메모리 할당 최적화)
            var corners = new[] { (0, 0), (0, 2), (2, 0), (2, 2) };

            // 전체 해제하되 제외할 코너만 skip
            for (int x = 0; x < 3; x++)
            {
                for (int y = 0; y < 3; y++)
                {
                    if (attack.excludedCornerIndex >= 0 && attack.excludedCornerIndex < corners.Length)
                    {
                        var (exX, exY) = corners[attack.excludedCornerIndex];
                        if (x == exX && y == exY) continue;
                    }
                    field[x][y].Unhightlight(Define.HighlightType.DefaultAttack);
                }
            }
        }

        public void AttackNoticeSign_Vertical(int lineNumber) // 세로 공격 왼쪽부터 pointNumber 0, 1, 2
        {
            for (int x = 0; x < 3; x++) // 가로 0,1,2 에 대해
            {
                field[x][lineNumber].Highlight(Define.HighlightType.DefaultAttack); // 해당 타일을 하이라이트 하기.
            }
            currentAttackStyle = AttackStyle.AttackVertical; // 포인트 어택 형태로 저장
        }

        public void AttackNoticeSign_Horizontal(int lineNumber) // 세로 공격 왼쪽부터 pointNumber 0, 1, 2
        {
            for (int x = 0; x < 3; x++) // 가로 0,1,2 에 대해
            {
                field[lineNumber][x].Highlight(Define.HighlightType.DefaultAttack); // 해당 타일을 하이라이트 하기.
            }
            currentAttackStyle = AttackStyle.AttackHorizontal; // 포인트 어택 형태로 저장
        }
        #endregion
    }
}