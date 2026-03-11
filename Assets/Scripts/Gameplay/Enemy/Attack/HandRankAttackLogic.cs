using Cardevil.Utils; // LogEx 사용 가정
using UnityEngine;
using System.Collections.Generic;

namespace Cardevil.InGame.Enemy
{
    public static class HandRankAttackLogic
    {
        #region 공격 설정

        public static void SetupAttack(Attack attack, IAttackVisualizer visualizer)
        {
            // 공격 패턴에 따라 분기
            switch (attack.currentAttackStyle)
            {
                case AttackStyle.HighCard: SetHighCard(attack, visualizer); break;
                case AttackStyle.OnePair: SetOnePair(attack, visualizer); break;
                case AttackStyle.TwoPair: SetTwoPair(attack, visualizer); break;
                case AttackStyle.Triple: SetTriple(attack, visualizer); break;
                case AttackStyle.Straight: SetStraight(attack, visualizer); break;
                case AttackStyle.Flush: SetFlush(attack, visualizer); break;
                case AttackStyle.FourCard: SetFourCard(attack, visualizer); break;
                case AttackStyle.StraightFlush: SetStraightFlush(attack, visualizer); break;

                // 이미 라인 공격으로 정해져 들어온 경우
                case AttackStyle.AttackHorizontal: SetHorizontal(attack, visualizer); break;
                case AttackStyle.AttackVertical: SetVertical(attack, visualizer); break;

                default:
                    LogEx.LogWarning($"Unknown Style: {attack.currentAttackStyle}, Default to HighCard");
                    SetHighCard(attack, visualizer);
                    break;
            }
        }

        // --- 개별 설정 메서드들 ---

        private static void SetHighCard(Attack attack, IAttackVisualizer visualizer)
        {
            if (attack.isPlayerAttack)
            {
                attack.attackPointNumber_x = (int)attack.playerPosition.i;
                attack.attackPointNumber_y = (int)attack.playerPosition.j;
            }
            else
            {
                attack.attackPointNumber_x = Random.Range(0, 3);
                attack.attackPointNumber_y = Random.Range(0, 3);
            }
            visualizer.ShowAttackSign_Point(attack.attackPointNumber_x, attack.attackPointNumber_y);
        }

        private static void SetOnePair(Attack attack, IAttackVisualizer visualizer)
        {
            // 메인 1개 + 추가 1개
            InitExtraArrays(attack, 1);

            // 메인 설정
            SetHighCard(attack, visualizer);

            // 추가 설정 
            SetRandomExtraPoint(attack, 0, (attack.attackPointNumber_x, attack.attackPointNumber_y));
            visualizer.ShowAttackSign_Point(attack.attackPointNumberExtra_x[0], attack.attackPointNumberExtra_y[0]);
        }

        private static void SetTwoPair(Attack attack, IAttackVisualizer visualizer)
        {
            // 메인 1개 + 추가 2개 (총 3개)
            InitExtraArrays(attack, 2);

            SetHighCard(attack, visualizer);

            // 첫 번째 엑스트라
            SetRandomExtraPoint(attack, 0, (attack.attackPointNumber_x, attack.attackPointNumber_y));
            visualizer.ShowAttackSign_Point(attack.attackPointNumberExtra_x[0], attack.attackPointNumberExtra_y[0]);

            // 두 번째 엑스트라 
            SetRandomExtraPoint(attack, 1,
                (attack.attackPointNumber_x, attack.attackPointNumber_y),
                (attack.attackPointNumberExtra_x[0], attack.attackPointNumberExtra_y[0]));
            visualizer.ShowAttackSign_Point(attack.attackPointNumberExtra_x[1], attack.attackPointNumberExtra_y[1]);
        }

        private static void SetTriple(Attack attack, IAttackVisualizer visualizer)
        {
            // 트리플은 랜덤하게 가로,세로, 대각선
            int randType = Random.Range(0, 3);

            if (randType == 0) // 가로 공격으로 전환
            {
                attack.currentAttackStyle = AttackStyle.AttackHorizontal;
                SetHorizontal(attack, visualizer);
            }
            else if (randType == 1) // 세로 공격으로 전환
            {
                attack.currentAttackStyle = AttackStyle.AttackVertical;
                SetVertical(attack, visualizer);
            }
            else // TwoPair공격처럼
            {
                SetTwoPair(attack, visualizer);
            }
        }

        private static void SetStraight(Attack attack, IAttackVisualizer visualizer)
        {
            // 스트레이트(5장) -> 가로줄(3장) + 랜덤 2개 점
            // 일단 가로줄 하나 긋고
            attack.currentAttackStyle = AttackStyle.AttackHorizontal; // 편의상 스타일 변경
            SetHorizontal(attack, visualizer); // attackLineNumber 설정됨

            // 추가 점 2개 설정 (라인 위에 있어도 상관없으면 그냥 둠, 겹치기 싫으면 로직 추가)
            InitExtraArrays(attack, 2);
            for (int i = 0; i < 2; i++)
            {
                attack.attackPointNumberExtra_x[i] = Random.Range(0, 3);
                attack.attackPointNumberExtra_y[i] = Random.Range(0, 3);
                visualizer.ShowAttackSign_Point(attack.attackPointNumberExtra_x[i], attack.attackPointNumberExtra_y[i]);
            }
        }

        private static void SetFlush(Attack attack, IAttackVisualizer visualizer)
        {
            // 플러쉬(같은무늬 5장) -> X자 형태 공격 (중앙 + 모서리 4개)
            // (0,0), (0,2), (1,1), (2,0), (2,2)

            // 중앙
            attack.attackPointNumber_x = 1;
            attack.attackPointNumber_y = 1;
            visualizer.ShowAttackSign_Point(1, 1);

            // 모서리 4개
            InitExtraArrays(attack, 4);
            int[] cx = { 0, 0, 2, 2 };
            int[] cy = { 0, 2, 0, 2 };

            for (int i = 0; i < 4; i++)
            {
                attack.attackPointNumberExtra_x[i] = cx[i];
                attack.attackPointNumberExtra_y[i] = cy[i];
                visualizer.ShowAttackSign_Point(cx[i], cy[i]);
            }
        }

        private static void SetFourCard(Attack attack, IAttackVisualizer visualizer)
        {
            // 포카드(4장) -> 메인 1개 + 추가 3개 (총 4개점)
            InitExtraArrays(attack, 3);

            // 메인
            SetHighCard(attack, visualizer);

            // 추가 3개 (중복 체크 간단하게 생략하거나 SetRandomExtraPoint 확장 사용)
            // 여기서는 간단히 랜덤 3개 뿌리겠습니다 (겹칠 수 있음 방지 로직은 생략됨)
            List<(int, int)> used = new List<(int, int)>();
            used.Add((attack.attackPointNumber_x, attack.attackPointNumber_y));

            for (int i = 0; i < 3; i++)
            {
                SetRandomExtraPoint(attack, i, used.ToArray());
                used.Add((attack.attackPointNumberExtra_x[i], attack.attackPointNumberExtra_y[i]));
                visualizer.ShowAttackSign_Point(attack.attackPointNumberExtra_x[i], attack.attackPointNumberExtra_y[i]);
            }
        }

        private static void SetStraightFlush(Attack attack, IAttackVisualizer visualizer)
        {
            // 스티플 -> 한 구석 빼고 전부 공격 (8칸)
            int safeCornerIdx = Random.Range(0, 4);
            attack.excludedCornerIndex = safeCornerIdx; // Attack 클래스에 필드 필요

            (int x, int y)[] corners = { (0, 0), (0, 2), (2, 0), (2, 2) };
            (int safeX, int safeY) = corners[safeCornerIdx];

            InitExtraArrays(attack, 7); // 메인(1) + 엑스트라(7) = 8

            int count = 0;
            bool mainSet = false;

            for (int x = 0; x < 3; x++)
            {
                for (int y = 0; y < 3; y++)
                {
                    if (x == safeX && y == safeY) continue; // 안전지대

                    visualizer.ShowAttackSign_Point(x, y);

                    if (!mainSet)
                    {
                        attack.attackPointNumber_x = x;
                        attack.attackPointNumber_y = y;
                        mainSet = true;
                    }
                    else
                    {
                        attack.attackPointNumberExtra_x[count] = x;
                        attack.attackPointNumberExtra_y[count] = y;
                        count++;
                    }
                }
            }
        }

        private static void SetHorizontal(Attack attack, IAttackVisualizer visualizer)
        {
            if (attack.isPlayerAttack)
                attack.attackLineNumber = (int)attack.playerPosition.i;
            else
                attack.attackLineNumber = Random.Range(0, 3);

            visualizer.ShowAttackSign_Horizontal(attack.attackLineNumber);
        }

        private static void SetVertical(Attack attack, IAttackVisualizer visualizer)
        {
            if (attack.isPlayerAttack)
                attack.attackLineNumber = (int)attack.playerPosition.j;
            else
                attack.attackLineNumber = Random.Range(0, 3);

            visualizer.ShowAttackSign_Vertical(attack.attackLineNumber);
        }

        #endregion

        #region 판정 로직 

        public static bool CheckHit(Attack attack, float damage, out (bool success, float dmg) resultInfo)
        {
            bool isHit = false;

            // AttackStyle에 따라 체크 (Triple, Straight 등이 Horizontal로 변했을 수 있으니 주의)
            switch (attack.currentAttackStyle)
            {
                case AttackStyle.AttackHorizontal: isHit = CheckHorizontal(attack); break;
                case AttackStyle.AttackVertical: isHit = CheckVertical(attack); break;

                // 포인트 기반 공격들은 검사 로직이 비슷함 (메인점 + 엑스트라점들)
                default:
                    isHit = CheckPointBasedAttack(attack);
                    break;
            }

            resultInfo = (isHit, isHit ? damage : 0);
            return isHit;
        }

        private static bool CheckHorizontal(Attack attack)
        {
            // 플레이어의 x좌표(행) == 공격 라인
            return attack.playerPosition.i == attack.attackLineNumber;
        }

        private static bool CheckVertical(Attack attack)
        {
            // 플레이어의 y좌표(열) == 공격 라인
            return attack.playerPosition.j == attack.attackLineNumber;
        }

        private static bool CheckPointBasedAttack(Attack attack)
        {
            // 1. 메인 포인트 검사
            if (IsPlayerAt(attack, attack.attackPointNumber_x, attack.attackPointNumber_y))
                return true;

            // 2. 엑스트라 포인트들 검사
            if (attack.attackPointNumberExtra_x != null)
            {
                for (int i = 0; i < attack.attackPointNumberExtra_x.Length; i++)
                {
                    if (IsPlayerAt(attack, attack.attackPointNumberExtra_x[i], attack.attackPointNumberExtra_y[i]))
                        return true;
                }
            }

            return false;
        }

        private static bool IsPlayerAt(Attack attack, int x, int y)
        {
            if (x < 0 || x > 2 || y < 0 || y > 2) return false;
            return (attack.playerPosition.i == x) &&
                   (attack.playerPosition.j == y);
        }

        #endregion

        #region Helpers

        private static void InitExtraArrays(Attack attack, int size)
        {
            attack.attackPointNumberExtra_x = new int[size];
            attack.attackPointNumberExtra_y = new int[size];
        }

        private static void SetRandomExtraPoint(Attack attack, int index, params (int x, int y)[] excludes)
        {
            int rx, ry;
            bool isDuplicate;
            int safetyCount = 0;

            do
            {
                isDuplicate = false;
                rx = Random.Range(0, 3);
                ry = Random.Range(0, 3);

                foreach (var ex in excludes)
                {
                    if (rx == ex.x && ry == ex.y)
                    {
                        isDuplicate = true;
                        break;
                    }
                }
                safetyCount++;
                if (safetyCount > 50) break; // 무한루프 방지
            } while (isDuplicate);

            attack.attackPointNumberExtra_x[index] = rx;
            attack.attackPointNumberExtra_y[index] = ry;
        }
        #endregion
    }
}