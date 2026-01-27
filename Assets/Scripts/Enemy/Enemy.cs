using Cardevil.Core.Turn;
using Cardevil.Ingame.Field;
using System;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Cardevil.Utils;
using UnityEngine.UI;
using Database.Generated;
using Cardevil.Events.ExecEvents;
using Cardevil.Events;
using Cardevil.Ingame.Entities;
using Cardevil.Ingame.Player;
using System.Threading;

namespace Cardevil.InGame.Enemy
{
    public enum AttackStyle
    {
        UnKnown,
        AttackPoint,
        AttackVertical,
        AttackHorizontal,
        HighCard,
        OnePair,
        TwoPair,
        Triple,
        Straight,
        Flush,
        FourCard,
        StraightFlush
    }
    public class Enemy : MonoBehaviour, TurnManager.ITurnTarget, IAttackVisualizer
    {

        //-----HP UI-----///
        [SerializeField] private Slider hpBar; // Inspector에서 UI Slider를 드래그하여 연결합니다.
        public float maxHP = 100;
        public BaseMobBossData baseMobBossData;

        private Field field;
        // ---- 기본 선언부 --- ///
        private float damage = 1; // Enemy의 공격력
        public float HP = 100; // Enemy의 체력
        public int attackCreateTurnOrder;
        public int attackCreateCycle = 3; // 일단 기본 3, 몇번 마다 공격이 시행되는지?
        private bool aWakeFirst = true;
        private bool isEnemyDead = false;
        public bool isPlayerAttack = true;
        private bool orderSettingGo = false;
        private int settingOrder = 3;
        public int delayAttackByRelic = 0;
        public List<Attack> attackLists = new List<Attack>();
        public (bool attackSucess, float damage) _enemyAttackInfo;
        private AttackStyle currentAttackStyle;
        private List<AttackStyle> attackStyles = new List<AttackStyle>(); // Enemy에게 지정된 공격할 AttackStyle

        // --------족보 관련----------
        public int enforcedAttackRanking = 0;
        public int enforcedAttackDamage = 0;

        // --------기믹 관련----------
        private IGimmick gimmick;
        public void Init(Field field)
        {
            this.field = field;
            currentAttackStyle = AttackStyle.UnKnown;
            maxHP = HP; // 시작 시 HP를 최대 HP로 저장합니다.
            UpdateHPBar(); // 시작 시 HP 바를 초기화합니다.

            int showDescription = (int)EnterStageArgs.Orders.ShowEnemyDescription;
            ExecStaticEventBus<EnterStageArgs>.Register(showDescription, OnShowDescriptionAsync);

            int showInitialAttackAreaPriority = (int)EnterStageArgs.Orders.ShowEnemyInitialAttackArea;
            ExecStaticEventBus<EnterStageArgs>.Register(showInitialAttackAreaPriority, OnShowInitialAttackAreaAsync);

            int attackPriority = (int)EnemyAttackArgs.Orders.EnemyAttack;
            ExecStaticEventBus<EnemyAttackArgs>.Register(attackPriority, OnTurnAttackAsync);

            int attackedPriority = (int)PlayerAttackArgs.Orders.EnemyAttacked;
            ExecStaticEventBus<PlayerAttackArgs>.Register(attackedPriority, OnTurnAttackedAsync);
            
            // TODO: 디스폰될 때 이벤트 해제
        }

        #region 이벤트 기반 적 행동

        private async UniTask OnShowDescriptionAsync(EnterStageArgs args, CancellationToken cancellationToken)
        {
            // 서인: 이거는 UI 관련된 거여서 Enemy 클래스에 있는게 맞는지는 모르겠음.
            // 대윤이 형이 관련된 구조 만들면서 "맨 처음에 적에 대한 설명 표시"를
            // 어디서 수행할지 정하고 수정해줘.
        }

        private async UniTask OnShowInitialAttackAreaAsync(EnterStageArgs args, CancellationToken cancellationToken)
        {
            // TODO: 첫 범위 표시하기
        }

        private async UniTask OnTurnAttackAsync(EnemyAttackArgs args, CancellationToken cancellationToken)
        {
            _enemyAttackInfo.attackSucess = false;
            
            // TODO:
            // 적이 공격하기
            // args.SetDamage(1);
            
            await UniTask.Delay(1200);
        }

        private async UniTask OnTurnAttackedAsync(PlayerAttackArgs args, CancellationToken cancellationToken)
        {
            // TODO: 적이 데미지 받기
            
            CurrentHp = HP - args.EvaluationResult.TotalDamage;
            LogEx.Log($"{args.EvaluationResult.TotalDamage}만큼의 피해를 입러 HP가 {CurrentHp}로 감소하였다!");
            UpdateHPBar();
            
            // 유닛 사망
            isEnemyDead = true;
        }

        #endregion

        // ITurnEnemy 변경 사항
        public async UniTask Replace()
        {
            LogEx.LogWarning("<b>대윤</b>: 아직 enemy 교체가 구현되지 않음.");
        }

        public async UniTask ShowInitialAttackArea(TileVector playerPosition)
        {
            AttackEnemyTurnStart(playerPosition);
        }
        
        // public async UniTask<AttackResult> TurnAttackAsync()
        // {
        //     _enemyAttackInfo.attackSucess = false;
        //
        //     var ctx = TurnManager.Context;
        //
        //     var target = ctx.Player;
        //     // 이제 플레이어 위치 이렇게 받아오면 됨!
        //     var playerPosition = ctx.PlayerPosition;
        //
        //     AttackEnemyTurnStart(ctx);
        //
        //     await UniTask.Delay(1200);
        //    
        //
        //     // TODO: 필요하다면 족보도 받아오기
        //     return _enemyAttackInfo.attackSucess
        //         ? new AttackResult(target, HandRanking.None, (int)damage + enforcedAttackDamage)
        //         : new AttackResult(target, HandRanking.None, 0);
        // }

        #region Attack 관련

        #region IAttackVisualizer 구현
        // HandRankAttackLogic에서 호출할 메서드들
        // 실제 구현되어 있는 AttackNoticeSign 함수들과 연결

        public void ShowAttackSign_Point(int x, int y)
        {
            AttackNoticeSign_Point(x, y);
        }

        public void ShowAttackSign_Horizontal(int line)
        {
            AttackNoticeSign_Horizontal(line);
        }

        public void ShowAttackSign_Vertical(int line)
        {
            AttackNoticeSign_Vertical(line);
        }
        #endregion


        public virtual void AttackEnemyAwake(TileVector playerPosition) // 처음으로 호출되었을때
        {
            if (aWakeFirst == true) // 처음에선 랜덤지정
            {
                CreateAttack(playerPosition, true);
            }
            aWakeFirst = false;
        }

        public void CreateAttack(TileVector playerPosition, bool firstCreate = false)
        {
            Attack tmpAttack = new() { playerPosition = playerPosition };

            tmpAttack.currentAttackStyle = SetAttackType(); // 무슨공격인지 설정 

            tmpAttack.SetAttackCycle(baseMobBossData.AttackCycle);

            if (firstCreate)
            {
                tmpAttack.attackTurnOrder++;
                tmpAttack.attackTurnOrder += delayAttackByRelic;

            }
            SetAttack(tmpAttack, baseMobBossData.AttackPlayer);
            attackLists.Add(tmpAttack); // 리스트에 어택추가
        }
        public void AttackEnemyTurnStart(TileVector playerPosition)
        {
            EnemyTurnClear();
            LogEx.Log("Enemy Turn!!");
            AttackEnemyAwake(playerPosition); // Enemy Awake시 실행되는 함수

            // 우선 시작할때 공격구역 설정하도록 해보기 Test


            AttackPatternEnemyTurning(); // Enemy가 수행하는 공격들의 TurnOrder 감소 후 공격

            AttackEnemyTurnEnd(playerPosition); // Enemy 공격의 마무리 단계
        }


        public void AttackPatternEnemyTurning()
        {
            if (HP <= 0)// 만약 Enemy의 체력이 0 이라면 공격을 수행하지않음
            {
                return;
            }
            // 다른 Enemy들이 더 존재하는지 확인 후 다음 스테이지로 

            foreach (Attack attack in attackLists) // 현재 Enemy가 가지고 있는 Attack 
            {
                attack.attackTurnOrder--; // 모든 Attack 들의 Turn Order 감소
                LogEx.Log($"다음 공격까지 {attack.attackTurnOrder}턴 남았습니다 - {attack.currentAttackStyle} : {attack.attackLineNumber}");
                if (attack.attackTurnOrder <= 0) // 0 이라면 공격시행
                {
                    AttackingCheck(attack);
                    RemoveHighLight(attack);
                }
            }

        }

        virtual public void AttackingCheck(Attack attack)
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
            // 공격이 끝난(TurnOrder == 0) 항목들에 대해 하이라이트 해제 먼저 수행
            foreach (var attack in attackLists)
            {
                if (attack.attackTurnOrder <= 0)
                    RemoveHighLight(attack);
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
            // 어택의 방식이 단일 공격이라면  콤보라면, 이 지정이 아니라 특별하게 나오는 순서같은게 지정될 수 있다.
            // 플레이어 턴으로 넘기기
        }


        /// <summary>
        /// true 라면 Player 위치를 받아와서 공격
        /// </summary>
        /// <param name="setPlayerAttack"></param>
        public virtual void SetAttack(Attack attack, bool setPlayerAttack = false)
        {
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
            LogEx.Log($"{damage}만큼의 피해를 입러 HP가 {CurrentHp}로 감소하였다!");
            UpdateHPBar();
            if (CurrentHp <= 0)
            {
                // 유닛 사망
                isEnemyDead = true;
                return true; // 사망시 true 변환 
            }

            return false; // 아직 살아있다
        }

        public bool IsDead
        {
            get { return isEnemyDead; }
        }

        public void TakeDamage(int amount)
        {
            GetDamage(amount);
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
            foreach (Attack attack in attackLists)
            {
                if (i <= attack.attackTurnOrder)
                {
                    attack.attackTurnOrder = i;
                    LogEx.Log($"attackTurnOrder이 {i}로 조정되었습니다.");
                }
                else
                {
                    LogEx.Log($"공격 턴오더 {i}가 현재 공격 턴 오더 {attack.attackTurnOrder} 보다 커서 조정되지않았습니다");
                }
            }
        }

        public void AttackOrderDiscount()
        {
            foreach (Attack attack in attackLists)
            {
                if (attack.attackTurnOrder <= 0)
                {
                    attack.attackTurnOrder--;
                    LogEx.Log("공격턴 추가 감소!");
                }
            }
        }

        /// <summary>
        /// Player위치로 공격하게끔
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
            
            if(baseMobBossData.BoolAttackType)
            {
                attackStyles = baseMobBossData.AttackPattern;
            }
            else
            {
                // 0이라면 가중치에따라 랜덤
               attackStyles= Util.PickRandomPatterns(
               baseMobBossData.AttackPattern,
               baseMobBossData.AttackWeight,
               15);
            }
            isPlayerAttack = baseMobBossData.AttackPlayer;

            //HP 설정
            maxHP = _baseMobBossData.BaseHP;
            CurrentHp = maxHP;

            SettingGimmick(_baseMobBossData);

        }

        private void SettingGimmick(BaseMobBossData baseMobBossData)
        {
            // TODO : 여러개 기믹이 존재하는 몹이 있으면 [0]을 수정해야함.

            char trimText = '"';
            IGimmick igimmick = GimmickFactory.Instance.CreateGimmick(baseMobBossData.GimmickName[0].ToString().Trim(trimText));
            gimmick = igimmick;
            if(igimmick==null)
            {
                return;
            }
            igimmick.Apply(this);
        }
        #endregion

        #region HighLight관련
        // 리팩토링 RemoveHighLight
        public void RemoveHighLight(Attack attack)
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

            // 스트레이트 플러시 (예외 케이스) 처리
            if (attack.currentAttackStyle == AttackStyle.StraightFlush)
            {
                RemoveHighLight_StraightFlush(attack);
                return;
            }

            // 나머지 점(Point) 기반 공격들 통합 처리
            // 메인 포인트 해제
            RemoveHighLight_Point(attack.attackPointNumber_x, attack.attackPointNumber_y);

            // 추가 포인트들 해제 (null 체크 포함하여 한 번에 처리)
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
            for (int x = 0; x < 3; x++) // 가로 0,1,2 에대해
            {
                field[lineNumber][x].Unhightlight(Define.HighlightType.DefaultAttack); // 해당 타일을 하이라이트하기.
            }
        }

        public void RemoveHighLight_Vertical(int lineNumber)
        {
            for (int x = 0; x < 3; x++) // 가로 0,1,2 에대해
            {
                field[x][lineNumber].Unhightlight(Define.HighlightType.DefaultAttack); // 해당 타일을 하이라이트하기.
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
                field[pointNumber_x][pointNumber_y].Highlight(Define.HighlightType.DefaultAttack); // 해당 타일을 하이라이트하기.
                currentAttackStyle = AttackStyle.AttackPoint; // 포인트 어택 형태로 저장
            }
        }

        // 스트레이트 플러시 전용 해제 로직 분리
        private void RemoveHighLight_StraightFlush(Attack attack)
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

        public void AttackNoticeSign_Vertical(int lineNumber) // 세로 공격 왼쪽부터 pointNumber 0,1,2
        {

            for (int x = 0; x < 3; x++) // 가로 0,1,2 에대해
            {
                field[x][lineNumber].Highlight(Define.HighlightType.DefaultAttack); // 해당 타일을 하이라이트하기.
            }
            currentAttackStyle = AttackStyle.AttackVertical; // 포인트 어택 형태로 저장
        }

        public void AttackNoticeSign_Horizontal(int lineNumber) // 세로 공격 왼쪽부터 pointNumber 0,1,2
        {
            for (int x = 0; x < 3; x++) // 가로 0,1,2 에대해
            {
                field[lineNumber][x].Highlight(Define.HighlightType.DefaultAttack); // 해당 타일을 하이라이트하기.
            }
            currentAttackStyle = AttackStyle.AttackHorizontal; // 포인트 어택 형태로 저장
        }

     


        #endregion


    }
}
