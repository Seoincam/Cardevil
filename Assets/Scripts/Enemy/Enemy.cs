using Cardevil.Ingame.Field;
using System;
using System.Collections.Generic;
using UnityEngine;
using Cardevil.Systems;
using System.Collections;
using Cysharp.Threading.Tasks;
using System.Threading;
using Cardevil.Ingame.Entities;

namespace Cardevil.InGame.Enemy
{
    //
    public class Enemy : MonoBehaviour, ITurnEnemy
    {
        private Field field;
        private float damage = 1; // Enemy의 공격력
        private float HP = 100; // Enemy의 체력
        public int attackCreateTurnOrder;
        public int attackCreateCycle = 3; // 일단 기본 3, 몇번 마다 공격이 시행되는지?
        private int attackLineNumber;
        private int attackPointNumber_x;
        private int attackPointNumber_y;
        private bool aWakeFirst = true;
        private bool isAttakced = false;
        private bool isEnemyDead = false;
        
        public bool isPlayerAttack = false;

        private List<Attack> attackLists = new List<Attack>();

        public enum AttackStyle
        {
            UnKnown,
            AttackPoint,
            AttackVertical,
            AttackHorizontal
        }
        private AttackStyle currentAttackStyle;

        public class Attack
        {
            public AttackStyle currentAttackStyle;
            public int attackLineNumber; // 어느곳에 공격하는지
            public int attackCycle = 3; // 몇번뒤에 공격하는지
            public int attackTurnOrder; // 현재 몇번뒤에 공격하는지
            public int damage = 1;
            public int attackPointNumber_x;
            public int attackPointNumber_y;

            public void SetAttackCycle()
            {
                attackTurnOrder = attackCycle; // 몇턴뒤에 공격
            }
            
        }
        private void Start()
        {
            field = Managers.Game.Field;
            currentAttackStyle = AttackStyle.UnKnown;
            Managers.Game.Enemy = this;

        }



        #region Attack 관련
        
        public bool CheckAttack()
        {
            foreach (Attack attack in attackLists) // 현재 Enemy가 가지고 있는 Attack 
            {
                if (attack.attackTurnOrder == 0) // 0 이라면 공격을 시행하는것이 있다.
                {
                    return true;
                }
            }
            return false;
        }
        public async UniTask TurnAttack() //인터페이스
        {
            AttackEnemyTurnStart();
        }
        public virtual void AttackEnemyAwake() // 처음으로 호출되었을때
        {
            if (aWakeFirst == true) // 처음에선 랜덤지정
            {
                CreateAttack(true);
            }
            aWakeFirst = false;
        }

        public void CreateAttack(bool firstCreate=false)
        {
            Attack tmpAttack = new Attack();
            tmpAttack.currentAttackStyle = SetAttackType();
            tmpAttack.SetAttackCycle();
            if(firstCreate) { tmpAttack.attackTurnOrder++; }
            SetAttack(tmpAttack,isPlayerAttack);
            attackLists.Add(tmpAttack); // 리스트에 어택추가
        }
        public void AttackEnemyTurnStart()
        {
            AttackEnemyAwake(); // Enemy Awake시 실행되는 함수

            // 우선 시작할때 공격구역 설정하도록 해보기 Test


            AttackPatternEnemyTurning(); // Enemy가 수행하는 공격들의 TurnOrder 감소 후 공격

            AttackEnemyTurnEnd(); // Enemy 공격의 마무리 단계
        }


        private void AttackPatternEnemyTurning()
        {
            if (HP <= 0)// 만약 Enemy의 체력이 0 이라면 공격을 수행하지않음
            {
                return;
            }
            // 다른 Enemy들이 더 존재하는지 확인 후 다음 스테이지로 

            foreach (Attack attack in attackLists) // 현재 Enemy가 가지고 있는 Attack 
            {
                attack.attackTurnOrder--; // 모든 Attack 들의 Turn Order 감소
                Debug.Log($"다음 공격까지 {attack.attackTurnOrder}턴 남았습니다 - {attack.currentAttackStyle} : {attack.attackLineNumber}");
                if (attack.attackTurnOrder == 0) // 0 이라면 공격시행
                {
                    AttackGo(attack);
                    RemoveHighLight(attack);
                }
            }



        }


        void AttackGo(Attack attack)
        {
            Debug.Log($"Enemy Attack! damage : {damage}");
            if (attack.currentAttackStyle == AttackStyle.AttackHorizontal) // 어택 타입에 따라 행동
            {
                AttackHorizontal(attackLineNumber);
            }
            else if (attack.currentAttackStyle == AttackStyle.AttackVertical)
            {
                AttackVerical(attackLineNumber);
            }
            else if (attack.currentAttackStyle == AttackStyle.AttackPoint)
            {
                AttackPoint(attackPointNumber_x, attackPointNumber_y);
            }
        }
        void AttackEnemyTurnEnd()
        {
            List<Attack> tmpAttacks = new List<Attack>();
            int count = 0;
            foreach(Attack attack in attackLists)
            {
                if (attack.attackTurnOrder == 0) // 이미 공격을 했다면
                {
                    // 새로운 공격 패턴 지정, 어택사이클 지정
                    RemoveHighLight(attack);
                    count++;
                }
                else
                {
                    tmpAttacks.Add(attack); // 공격을 진행하지 않은 Attack만 저장
                }
            }

            attackLists = tmpAttacks; // 어택리스트 재조정

            for (int i=0;i<count;i++) // 지워진 어택 갯수만큼 새로 생성
            {
                CreateAttack();
            }
           
            
   
            


            // 어택의 방식이 단일 공격이라면  콤보라면, 이 지정이 아니라 특별하게 나오는 순서같은게 지정될 수 있다.

            // 플레이어 턴으로 넘기기
        }


        void SetAttackLine() // 어느 라인에 공격할건지 정하기
        {

        }

        /// <summary>
        /// true 라면 Player 위치를 받아와서 공격
        /// </summary>
        /// <param name="setPlayerAttack"></param>
        virtual public void SetAttack(Attack attack,bool setPlayerAttack = false)
        {
            if (setPlayerAttack) // 플레이어 위치로 공격할 것인가에 대해
            {
                SetPlayerAttack(attack);
            }
            else
            {
                SetRandomAttack(attack);
            }



           if (attack.currentAttackStyle == AttackStyle.AttackPoint) // 포인트 공격이 진행된다면.
            {
                // 포인트 랜덤으로 지정
                attack.attackPointNumber_y = UnityEngine.Random.Range(0, 2);
                attack.attackPointNumber_x = attack.attackLineNumber;
                AttackNoticeSign_Point(attack.attackPointNumber_x, attack.attackPointNumber_y);
            }
            else { Debug.Log("currentAttackStyle을 찾지 못한 오류"); }
            Debug.Log($"Attack 예상 sign {attack.currentAttackStyle}");
        }
        /// <summary>
        /// 플레이어위치로 공격설정
        /// </summary>
        /// <param name="attack"></param>
        public void SetPlayerAttack(Attack attack)
        {
            // 현재 가로공격인지 세로공격인지 확인
            if (attack.currentAttackStyle == AttackStyle.AttackHorizontal) // 가로공격
            {
                //플레이어의 가로 위치
                attack.attackLineNumber = Managers.Game.Player.GetPlayerLineNumberHorizontal();
                //공격위치표시
                AttackNoticeSign_Horizontal(attack.attackLineNumber);

            }
            else if (attack.currentAttackStyle == AttackStyle.AttackVertical) // 세로공격
            {
                //플레이어의 세로 위치
                attack.attackLineNumber = Managers.Game.Player.GetPlayerLineNumberVertical();
                //공격위치표시
                AttackNoticeSign_Vertical(attack.attackLineNumber);
            }
        }

        /// <summary>
        /// 랜덤위치로 공격 설정
        /// </summary>
        /// <returns></returns>
        void SetRandomAttack(Attack attack)
        {
            attack.attackLineNumber = UnityEngine.Random.Range(0, 2); // 랜덤으로 위치 지정
        }
        AttackStyle SetAttackType()
        {
            currentAttackStyle = GetRandomAttackStyle(); // 랜덤으로 어택스타일 받기;
            return currentAttackStyle;
        }
      
        AttackStyle GetRandomAttackStyle() // 랜덤으로 어택스타일 받기
        {
            Array values = Enum.GetValues(typeof(AttackStyle));
            return (isPlayerAttack)?(AttackStyle)values.GetValue(UnityEngine.Random.Range(2, values.Length)): (AttackStyle)values.GetValue(UnityEngine.Random.Range(1, values.Length));
            // 랜덤값 다르게받기
        }

        #region HighLight관련
        public void RemoveHighLight(Attack attack)
        {
            if (attack.currentAttackStyle == AttackStyle.AttackHorizontal) // 어택 타입에 따라 행동
            {
                RemoveHighLight_Horizontal(attack.attackLineNumber);
            }
            else if (attack.currentAttackStyle == AttackStyle.AttackVertical)
            {
                RemoveHighLight_Vertical(attack.attackLineNumber);
            }
            else if (attack.currentAttackStyle == AttackStyle.AttackPoint)
            {
                RemoveHighLight_Point(attack.attackPointNumber_x, attack.attackPointNumber_y);
            }
        }

        public void RemoveHighLight_Horizontal(int lineNumber)
        {
            for (int x = 0; x < 3; x++) // 가로 0,1,2 에대해
            {
                field[x][lineNumber].UnHighLightAttackTile(); // 해당 타일을 하이라이트하기.
            }
        }

        public void RemoveHighLight_Vertical(int lineNumber)
        {
            for (int x = 0; x < 3; x++) // 가로 0,1,2 에대해
            {
                field[lineNumber][x].UnHighLightAttackTile(); // 해당 타일을 하이라이트하기.
            }
        }
        void RemoveHighLight_Point(int pointNumber_x, int pointNumber_y)
        {
            field[pointNumber_x][pointNumber_y].UnHighLightAttackTile(); // 해당 타일 하이라이트 off
        }
        public void AttackNoticeSign_Point(int pointNumber_x, int pointNumber_y)
        {
            field[pointNumber_x][pointNumber_y].HighLightAttackTile(); // 해당 타일을 하이라이트하기.
            currentAttackStyle = AttackStyle.AttackPoint; // 포인트 어택 형태로 저장
        }

        public void AttackNoticeSign_Vertical(int lineNumber) // 세로 공격 왼쪽부터 pointNumber 0,1,2
        {

            for (int x = 0; x < 3; x++) // 가로 0,1,2 에대해
            {
                field[lineNumber][x].HighLightAttackTile(); // 해당 타일을 하이라이트하기.
            }
            currentAttackStyle = AttackStyle.AttackVertical; // 포인트 어택 형태로 저장
        }

        public void AttackNoticeSign_Horizontal(int lineNumber) // 세로 공격 왼쪽부터 pointNumber 0,1,2
        {
            for (int x = 0; x < 3; x++) // 가로 0,1,2 에대해
            {
                field[x][lineNumber].HighLightAttackTile(); // 해당 타일을 하이라이트하기.
            }
            currentAttackStyle = AttackStyle.AttackHorizontal; // 포인트 어택 형태로 저장
        }


        #endregion

        // 실질적인 공격 후 데미지 주기
        void AttackPoint(int pointNumber_x, int poinNumber_y)
        {
            List<Entity> entities =
            field[pointNumber_x][poinNumber_y].GetEntities(); // 찾아보는 타일에 있는 Entity 받아오기

            //Entity중 Player가 있다면
            foreach (Entity entity in entities)
            {
                if (entity.TryGetComponent<PlayerCharacter>(out var player)) // 존재하는걸 확인했다면
                {
                    // PlayerCharacter가 ITurnPlayerAction을 구현중임.
                    if (player is ITurnPlayerAction action)
                    {
                        action.PlayerGetDamage(damage);
                    }
                }
            }

        }

        void AttackVerical(int pointNumber) // 세로 공격 왼쪽부터 pointNumber 0,1,2
        {
            // 가로는 0,1,2 모두
            for (int x = 0; x < 3; x++)
            {
                List<Entity> entities =
                field[pointNumber][x].GetEntities(); // 찾아보는 타일에 있는 Entity 받아오기

                //Entity중 Player가 있다면
                foreach(Entity entity in entities)
                {
                    if(entity.TryGetComponent<PlayerCharacter>(out var player)) // 존재하는걸 확인했다면
                    {
                        // PlayerCharacter가 ITurnPlayerAction을 구현중임.
                        if (player is ITurnPlayerAction action)
                        {
                            action.PlayerGetDamage(damage);
                        }
                    }
                }

            }
        }


        void AttackHorizontal(int pointNumber) // 가로 공격 왼쪽부터 pointNumber 0,1,2
        {
            // 가로는 0,1,2 모두
            for (int x = 0; x < 3; x++)
            {
                List<Entity> entities =
               field[pointNumber][x].GetEntities(); // 찾아보는 타일에 있는 Entity 받아오기

                //Entity중 Player가 있다면
                foreach (Entity entity in entities)
                {
                    if (entity.TryGetComponent<PlayerCharacter>(out var player)) // 존재하는걸 확인했다면
                    {
                        // PlayerCharacter가 ITurnPlayerAction을 구현중임.
                        if (player is ITurnPlayerAction action)
                        {
                            action.PlayerGetDamage(damage);
                        }
                    }
                }

            }
        }
        #endregion

        public virtual bool GetDamage(float damage)
        {
            HP -= damage;
            Debug.Log($"{damage}만큼의 피해를 입러 HP가 {HP}로 감소하였다!");
            if (HP <= 0)
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


        #region Tool 관련
        public void SetAllAttackOrder(int i)
        {
            foreach (Attack attack in attackLists)
            {
                if (i <= attack.attackTurnOrder)
                {
                    attack.attackTurnOrder = i;
                    Debug.Log($"attackTurnOrder이 {i}로 조정되었습니다.");
                }
                else
                {
                    Debug.Log($"공격 턴오더 {i}가 현재 공격 턴 오더 {attack.attackTurnOrder} 보다 커서 조정되지않았습니다");
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
                    Debug.Log("공격턴 추가 감소!");
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

        public void TurnClear()
        {
            isAttakced = false;
        }
        public void IsAttacked(int amount)
        {
            isAttakced = true;
            GetDamage(amount);
        }
        #endregion
    }
}
