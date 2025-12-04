using Cardevil.Ingame.Field;
using System;
using System.Collections.Generic;
using UnityEngine;
using Cardevil.Systems;
using System.Collections;
using Cysharp.Threading.Tasks;
using System.Threading;
using Cardevil.Ingame.Entities;
using Cardevil.Ingame.Player;
using Cardevil.Utils;
using UnityEngine.UI;
using Database.Generated;

namespace Cardevil.InGame.Enemy
{
    //
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
        StraightPlush
    }
    public class Enemy : MonoBehaviour, ITurnEnemy
    {

        //-----HP UI-----///
        [SerializeField] private Slider hpBar; // Inspector에서 UI Slider를 드래그하여 연결합니다.
        private float maxHP = 100;
        private BaseMobBossData baseMobBossData;

        private Field field;
        private float damage = 1; // Enemy의 공격력
        public float HP = 100; // Enemy의 체력
        public int attackCreateTurnOrder;
        public int attackCreateCycle = 3; // 일단 기본 3, 몇번 마다 공격이 시행되는지?
        private bool aWakeFirst = true;
        private bool isAttakced = false;
        public bool isAttackSuccess = false;
        private bool isEnemyDead = false;

        public bool isPlayerAttack = true;
        private bool orderSettingGo = false;
        private int settingOrder = 3;
        public int delayAttackByRelic = 0;

        private List<Attack> attackLists = new List<Attack>();



        private AttackStyle currentAttackStyle;

        public class Attack
        {
            public AttackStyle currentAttackStyle;
            public int attackLineNumber; // 어느곳에 공격하는지
            public int attackCycle = 3; // 몇번뒤에 공격하는지
            public int attackTurnOrder; // 현재 몇번뒤에 공격하는지
            public int damage = 1;
            public int attackPointNumber_x = 0;
            public int attackPointNumber_y = 0;
            public int[] attackPointNumberExtra_x;
            public int[] attackPointNumberExtra_y;
            public bool isPlayerAttack = true;

            // 추가: StraightPlush 등에서 "제외한 코너"를 기억하기 위한 필드
            // -1 이면 아직 설정되지 않음
            public int excludedCornerIndex = -1;

            public void SetAttackCycle(int cycle)
            {
                attackCycle = cycle;
                LogEx.Log($"어택 턴오더 {attackTurnOrder} = 어택 사이클{attackCycle}");
                attackTurnOrder = attackCycle; // 몇턴뒤에 공격
            }

        }
        private void Start()
        {
            field = Managers.Game.Field;
            currentAttackStyle = AttackStyle.UnKnown;
            maxHP = HP; // 시작 시 HP를 최대 HP로 저장합니다.
            UpdateHPBar(); // 시작 시 HP 바를 초기화합니다.

        }

        private void Awake()
        {
        }

        #region 족보공격 구현

        /// <summary>
        /// 하이카드 공격
        /// </summary>
        /// <returns></returns>
        private void SettingAttackHighCard(Attack attack)
        {
            LogEx.Log("SetHighCardAttack!");

            attack.currentAttackStyle = AttackStyle.HighCard;

            if (attack.isPlayerAttack)
            {
                attack.attackPointNumber_x = Managers.Game.Player.GetPlayerLineNumberHorizontal();
                attack.attackPointNumber_y = Managers.Game.Player.GetPlayerLineNumberVertical();
            }
            else
            {
                attack.attackPointNumber_x = UnityEngine.Random.Range(0, 3);
                attack.attackPointNumber_y = UnityEngine.Random.Range(0, 3);
            }
            //공격위치표시
            AttackNoticeSign_Point(attack.attackPointNumber_x, attack.attackPointNumber_y);
        }

        private void SettingAttackOnePare(Attack attack)
        {
            LogEx.Log("SetOnePareAttack!");

            attack.currentAttackStyle = AttackStyle.OnePair;

            // ensure extras arrays allocated
            attack.attackPointNumberExtra_x = new int[1];
            attack.attackPointNumberExtra_y = new int[1];

            if (attack.isPlayerAttack)
            {
                int px = Managers.Game.Player.GetPlayerLineNumberHorizontal();
                int py = Managers.Game.Player.GetPlayerLineNumberVertical();
                attack.attackPointNumber_x = px;
                attack.attackPointNumber_y = py;

                // pick a random tile different from player's tile
                int rx, ry;
                do
                {
                    rx = UnityEngine.Random.Range(0, 3);
                    ry = UnityEngine.Random.Range(0, 3);
                } while (rx == attack.attackPointNumber_x && ry == attack.attackPointNumber_y);

                attack.attackPointNumberExtra_x[0] = rx;
                attack.attackPointNumberExtra_y[0] = ry;

                // highlight
                AttackNoticeSign_Point(attack.attackPointNumber_x, attack.attackPointNumber_y);
                AttackNoticeSign_Point(attack.attackPointNumberExtra_x[0], attack.attackPointNumberExtra_y[0]);
            }
            else
            {
                // random mode: choose two random tiles but only one extra used here
                attack.attackPointNumber_x = UnityEngine.Random.Range(0, 3);
                attack.attackPointNumber_y = UnityEngine.Random.Range(0, 3);
                int rx = UnityEngine.Random.Range(0, 3);
                int ry = UnityEngine.Random.Range(0, 3);
                if (rx == attack.attackPointNumber_x && ry == attack.attackPointNumber_y)
                {
                    // ensure different
                    rx = (rx + 1) % 3;
                }
                attack.attackPointNumberExtra_x[0] = rx;
                attack.attackPointNumberExtra_y[0] = ry;
                AttackNoticeSign_Point(attack.attackPointNumber_x, attack.attackPointNumber_y);
                AttackNoticeSign_Point(rx, ry);
            }

        }

        private void SettingAttackTwoPare(Attack attack)
        {
            LogEx.Log("SetTwoPareAttack!");

            attack.currentAttackStyle = AttackStyle.TwoPair;

            // allocate extras for two points
            attack.attackPointNumberExtra_x = new int[2];
            attack.attackPointNumberExtra_y = new int[2];

            if (attack.isPlayerAttack)
            {
                attack.attackPointNumber_x = Managers.Game.Player.GetPlayerLineNumberHorizontal();
                attack.attackPointNumber_y = Managers.Game.Player.GetPlayerLineNumberVertical();

                // pick two distinct random tiles excluding player's tile
                int count = 0;
                while (count < 2)
                {
                    int rx = UnityEngine.Random.Range(0, 3);
                    int ry = UnityEngine.Random.Range(0, 3);

                    // skip if equals player or duplicates
                    bool equalsPlayer = (rx == attack.attackPointNumber_x && ry == attack.attackPointNumber_y);
                    bool duplicate = false;
                    for (int i = 0; i < count; i++)
                    {
                        if (rx == attack.attackPointNumberExtra_x[i] && ry == attack.attackPointNumberExtra_y[i]) { duplicate = true; break; }
                    }
                    if (equalsPlayer || duplicate) continue;

                    attack.attackPointNumberExtra_x[count] = rx;
                    attack.attackPointNumberExtra_y[count] = ry;
                    count++;
                }

                // highlight
                AttackNoticeSign_Point(attack.attackPointNumber_x, attack.attackPointNumber_y);
                AttackNoticeSign_Point(attack.attackPointNumberExtra_x[0], attack.attackPointNumberExtra_y[0]);
                AttackNoticeSign_Point(attack.attackPointNumberExtra_x[1], attack.attackPointNumberExtra_y[1]);
            }
            else
            {
                // random fallback similar logic
                attack.attackPointNumber_x = UnityEngine.Random.Range(0, 3);
                attack.attackPointNumber_y = UnityEngine.Random.Range(0, 3);
                int count = 0;
                while (count < 2)
                {
                    int rx = UnityEngine.Random.Range(0, 3);
                    int ry = UnityEngine.Random.Range(0, 3);
                    bool equalsPlayer = (rx == attack.attackPointNumber_x && ry == attack.attackPointNumber_y);
                    bool duplicate = false;
                    for (int i = 0; i < count; i++)
                    {
                        if (rx == attack.attackPointNumberExtra_x[i] && ry == attack.attackPointNumberExtra_y[i]) { duplicate = true; break; }
                    }
                    if (equalsPlayer || duplicate) continue;
                    attack.attackPointNumberExtra_x[count] = rx;
                    attack.attackPointNumberExtra_y[count] = ry;
                    count++;
                }
                AttackNoticeSign_Point(attack.attackPointNumber_x, attack.attackPointNumber_y);
                AttackNoticeSign_Point(attack.attackPointNumberExtra_x[0], attack.attackPointNumberExtra_y[0]);
                AttackNoticeSign_Point(attack.attackPointNumberExtra_x[1], attack.attackPointNumberExtra_y[1]);
            }
        }

        private void SettingAttackTriple(Attack attack)
        {
            LogEx.Log("SetTripleAttack!");

            // Triple : 플레이어 위치를 기준으로 '행 or 열' 공격 (둘 중 하나)
            attack.currentAttackStyle = AttackStyle.Triple;

            bool horizontal;
            if (attack.isPlayerAttack)
            {
                horizontal = (UnityEngine.Random.value > 0.5f);
                if (horizontal)
                {
                    attack.attackLineNumber = Managers.Game.Player.GetPlayerLineNumberHorizontal();
                    attack.currentAttackStyle = AttackStyle.AttackHorizontal;
                    AttackNoticeSign_Horizontal(attack.attackLineNumber);
                }
                else
                {
                    attack.attackLineNumber = Managers.Game.Player.GetPlayerLineNumberVertical();
                    attack.currentAttackStyle = AttackStyle.AttackVertical;
                    AttackNoticeSign_Vertical(attack.attackLineNumber);
                }
            }
            else
            {
                // random row/col
                horizontal = (UnityEngine.Random.value > 0.5f);
                if (horizontal)
                {
                    attack.attackLineNumber = UnityEngine.Random.Range(0, 3);
                    attack.currentAttackStyle = AttackStyle.AttackHorizontal;
                    AttackNoticeSign_Horizontal(attack.attackLineNumber);
                }
                else
                {
                    attack.attackLineNumber = UnityEngine.Random.Range(0, 3);
                    attack.currentAttackStyle = AttackStyle.AttackVertical;
                    AttackNoticeSign_Vertical(attack.attackLineNumber);
                }
            }
        }

        private void SettingAttackStraight(Attack attack)
        {
            LogEx.Log("SetStraightAttack!");

            // Straight : (행 or 열) 공격 + 랜덤 1x1 공격
            // allocate one extra
            attack.attackPointNumberExtra_x = new int[1];
            attack.attackPointNumberExtra_y = new int[1];

            attack.currentAttackStyle = AttackStyle.Straight;

            // choose orientation same as Triple logic
            bool horizontal = (attack.isPlayerAttack) ? (UnityEngine.Random.value > 0.5f) : (UnityEngine.Random.value > 0.5f);

            if (attack.isPlayerAttack)
            {
                if (horizontal)
                {
                    attack.attackLineNumber = Managers.Game.Player.GetPlayerLineNumberHorizontal();
                    attack.currentAttackStyle = AttackStyle.AttackHorizontal;
                    AttackNoticeSign_Horizontal(attack.attackLineNumber);
                }
                else
                {
                    attack.attackLineNumber = Managers.Game.Player.GetPlayerLineNumberVertical();
                    attack.currentAttackStyle = AttackStyle.AttackVertical;
                    AttackNoticeSign_Vertical(attack.attackLineNumber);
                }

                // random extra tile excluding player's tile
                int rx, ry;
                int px = Managers.Game.Player.GetPlayerLineNumberHorizontal();
                int py = Managers.Game.Player.GetPlayerLineNumberVertical();
                do
                {
                    rx = UnityEngine.Random.Range(0, 3);
                    ry = UnityEngine.Random.Range(0, 3);
                } while (rx == px && ry == py);
                attack.attackPointNumberExtra_x[0] = rx;
                attack.attackPointNumberExtra_y[0] = ry;
                AttackNoticeSign_Point(rx, ry);
            }
            else
            {
                // random orientation and row/col index
                if (horizontal)
                {
                    attack.attackLineNumber = UnityEngine.Random.Range(0, 3);
                    attack.currentAttackStyle = AttackStyle.AttackHorizontal;
                    AttackNoticeSign_Horizontal(attack.attackLineNumber);
                }
                else
                {
                    attack.attackLineNumber = UnityEngine.Random.Range(0, 3);
                    attack.currentAttackStyle = AttackStyle.AttackVertical;
                    AttackNoticeSign_Vertical(attack.attackLineNumber);
                }

                int rx = UnityEngine.Random.Range(0, 3);
                int ry = UnityEngine.Random.Range(0, 3);
                attack.attackPointNumberExtra_x[0] = rx;
                attack.attackPointNumberExtra_y[0] = ry;
                AttackNoticeSign_Point(rx, ry);
            }
        }

        private void SettingAttackPlush(Attack attack)
        {
            LogEx.Log("SetPlushAttack! (2x2)");

            attack.currentAttackStyle = AttackStyle.Flush;

            // Flush : 2x2 square that includes player's tile (but clamped inside 3x3)
            attack.attackPointNumberExtra_x = new int[3]; // we will store the other 3 tiles; main point stored in attackPointNumber_x/y
            attack.attackPointNumberExtra_y = new int[3];

            int px, py;
            if (attack.isPlayerAttack)
            {
                px = Managers.Game.Player.GetPlayerLineNumberHorizontal();
                py = Managers.Game.Player.GetPlayerLineNumberVertical();
            }
            else
            {
                px = UnityEngine.Random.Range(0, 3);
                py = UnityEngine.Random.Range(0, 3);
            }

            // Choose top-left corner of 2x2 such that player's tile is inside and corner in [0,1]
            int startX = Mathf.Clamp(px - UnityEngine.Random.Range(0, 2), 0, 1);
            int startY = Mathf.Clamp(py - UnityEngine.Random.Range(0, 2), 0, 1);

            // main point: set attackPointNumber_x/y to player's tile coordinates for damage centering
            attack.attackPointNumber_x = px;
            attack.attackPointNumber_y = py;

            // collect all 4 tiles
            List<(int x, int y)> tiles = new List<(int, int)>();
            for (int dx = 0; dx <= 1; dx++)
            {
                for (int dy = 0; dy <= 1; dy++)
                {
                    tiles.Add((startX + dx, startY + dy));
                }
            }

            // highlight all 4 tiles - use Point highlight for each
            for (int i = 0; i < tiles.Count; i++)
            {
                var t = tiles[i];
                AttackNoticeSign_Point(t.x, t.y);
            }

            // store extras = the other three tile coords
            int idx = 0;
            for (int i = 0; i < tiles.Count; i++)
            {
                var t = tiles[i];
                if (t.x == px && t.y == py) continue;
                attack.attackPointNumberExtra_x[idx] = t.x;
                attack.attackPointNumberExtra_y[idx] = t.y;
                idx++;
            }
        }

        private void SettingAttackFourCard(Attack attack)
        {
            LogEx.Log("SetFourCardAttack! (cross)");

            attack.currentAttackStyle = AttackStyle.FourCard;

            // FourCard : center on player's tile and always cross (center + up/down/left/right if exist)
            attack.attackPointNumberExtra_x = new int[4];
            attack.attackPointNumberExtra_y = new int[4];

            int cx, cy;
            if (attack.isPlayerAttack)
            {
                cx = Managers.Game.Player.GetPlayerLineNumberHorizontal();
                cy = Managers.Game.Player.GetPlayerLineNumberVertical();
            }
            else
            {
                cx = UnityEngine.Random.Range(0, 3);
                cy = UnityEngine.Random.Range(0, 3);
            }

            attack.attackPointNumber_x = cx;
            attack.attackPointNumber_y = cy;

            // highlight center
            AttackNoticeSign_Point(cx, cy);

            // four directions
            int idx = 0;
            (int, int)[] dirs = new (int, int)[] { (0, 1), (0, -1), (1, 0), (-1, 0) }; // down, up, right, left
            foreach (var d in dirs)
            {
                int nx = cx + d.Item1;
                int ny = cy + d.Item2;
                if (nx >= 0 && nx < 3 && ny >= 0 && ny < 3)
                {
                    attack.attackPointNumberExtra_x[idx] = nx;
                    attack.attackPointNumberExtra_y[idx] = ny;
                    AttackNoticeSign_Point(nx, ny);
                    idx++;
                }
            }

            // if fewer than 4 valid neighbours, remaining slots left as 0 (Attack handler will check bounds)
        }

        private void SettingAttackStraightPlush(Attack attack)
        {
            LogEx.Log("SetStraightPlushAttack! (center all except one diagonal)");

            attack.currentAttackStyle = AttackStyle.StraightPlush;

            // 중앙 고정
            attack.attackPointNumber_x = 1;
            attack.attackPointNumber_y = 1;

            // 대각선 코너 4개 중 하나를 제외하도록 랜덤 선택하고 저장
            List<(int x, int y)> corners = new List<(int, int)> { (0, 0), (0, 2), (2, 0), (2, 2) };
            int excludeIndex = UnityEngine.Random.Range(0, corners.Count);
            attack.excludedCornerIndex = excludeIndex;

            var excludedCorner = corners[excludeIndex];
            int excludedX = excludedCorner.x;
            int excludedY = excludedCorner.y;

            // 제외한 칸을 빼고 나머지 칸들을 하이라이트
            for (int x = 0; x < 3; x++)
            {
                for (int y = 0; y < 3; y++)
                {
                    if (x == excludedX && y == excludedY) continue;
                    AttackNoticeSign_Point(x, y);
                }
            }
        }

        private bool AttackHighCard(Attack attack)
        {
            bool successAttack = false;

            if (!IsValidPoint(attack.attackPointNumber_x, attack.attackPointNumber_y)) return false;

            List<Entity> entities =
            field[attack.attackPointNumber_x][attack.attackPointNumber_y].GetEntities(); // 찾아보는 타일에 있는 Entity 받아오기
            //Entity중 Player가 있다면
            foreach (Entity entity in entities)
            {
                if (entity.TryGetComponent<PlayerCharacter>(out var player)) // 존재하는걸 확인했다면
                {
                    // PlayerCharacter가 ITurnPlayerAction을 구현중임.
                    if (player is ITurnPlayer action)
                    {
                        action.PlayerGetDamage(damage);
                        successAttack = true;
                    }
                }
            }
            return successAttack;

        }
        private bool AttackOnePare(Attack attack)
        {
            bool successAttack = false;

            // main
            if (IsValidPoint(attack.attackPointNumber_x, attack.attackPointNumber_y))
            {
                var entitiesMain = field[attack.attackPointNumber_x][attack.attackPointNumber_y].GetEntities();
                foreach (Entity entity in entitiesMain)
                {
                    if (entity.TryGetComponent<PlayerCharacter>(out var player))
                    {
                        if (player is ITurnPlayer action)
                        {
                            action.PlayerGetDamage(damage);
                            successAttack = true;
                        }
                    }
                }
            }

            // extra[0]
            if (attack.attackPointNumberExtra_x != null && attack.attackPointNumberExtra_x.Length > 0)
            {
                int ex = attack.attackPointNumberExtra_x[0];
                int ey = attack.attackPointNumberExtra_y[0];
                if (IsValidPoint(ex, ey))
                {
                    var entitiesExtra = field[ex][ey].GetEntities();
                    foreach (Entity entity in entitiesExtra)
                    {
                        if (entity.TryGetComponent<PlayerCharacter>(out var player))
                        {
                            if (player is ITurnPlayer action)
                            {
                                action.PlayerGetDamage(damage);
                                successAttack = true;
                            }
                        }
                    }
                }
            }

            return successAttack;
        }

        private bool AttackTwoPare(Attack attack)
        {
            bool successAttack = false;

            // main
            if (IsValidPoint(attack.attackPointNumber_x, attack.attackPointNumber_y))
            {
                var entitiesMain = field[attack.attackPointNumber_x][attack.attackPointNumber_y].GetEntities();
                foreach (Entity entity in entitiesMain)
                {
                    if (entity.TryGetComponent<PlayerCharacter>(out var player))
                    {
                        if (player is ITurnPlayer action)
                        {
                            action.PlayerGetDamage(damage);
                            successAttack = true;
                        }
                    }
                }
            }

            // extras (up to 2)
            if (attack.attackPointNumberExtra_x != null)
            {
                int len = attack.attackPointNumberExtra_x.Length;
                for (int i = 0; i < len; i++)
                {
                    int ex = attack.attackPointNumberExtra_x[i];
                    int ey = attack.attackPointNumberExtra_y[i];
                    if (!IsValidPoint(ex, ey)) continue;
                    var entitiesExtra = field[ex][ey].GetEntities();
                    foreach (Entity entity in entitiesExtra)
                    {
                        if (entity.TryGetComponent<PlayerCharacter>(out var player))
                        {
                            if (player is ITurnPlayer action)
                            {
                                action.PlayerGetDamage(damage);
                                successAttack = true;
                            }
                        }
                    }
                }
            }

            return successAttack;
        }

        private bool AttackTriple(Attack attack)
        {
            // Triple은 보통 SettingAttackTriple에서 currentAttackStyle을 AttackHorizontal 또는 AttackVertical로 바꿔줌.
            if (attack.currentAttackStyle == AttackStyle.AttackHorizontal)
            {
                return AttackHorizontal(attack.attackLineNumber);
            }
            else if (attack.currentAttackStyle == AttackStyle.AttackVertical)
            {
                return AttackVerical(attack.attackLineNumber);
            }
            else
            {
                // fallback: treat as point on player's tile
                return AttackHighCard(attack);
            }
        }

        private bool AttackStraight(Attack attack)
        {
            bool successAttack = false;

            // First perform row/col if currentAttackStyle was switched to horizontal/vertical
            if (attack.currentAttackStyle == AttackStyle.AttackHorizontal)
            {
                successAttack |= AttackHorizontal(attack.attackLineNumber);
            }
            else if (attack.currentAttackStyle == AttackStyle.AttackVertical)
            {
                successAttack |= AttackVerical(attack.attackLineNumber);
            }

            // then extra 1x1 if exists
            if (attack.attackPointNumberExtra_x != null && attack.attackPointNumberExtra_x.Length > 0)
            {
                int ex = attack.attackPointNumberExtra_x[0];
                int ey = attack.attackPointNumberExtra_y[0];
                if (IsValidPoint(ex, ey))
                {
                    var entities = field[ex][ey].GetEntities();
                    foreach (Entity entity in entities)
                    {
                        if (entity.TryGetComponent<PlayerCharacter>(out var player))
                        {
                            if (player is ITurnPlayer action)
                            {
                                action.PlayerGetDamage(damage);
                                successAttack = true;
                            }
                        }
                    }
                }
            }

            return successAttack;
        }

        private bool AttackPlush(Attack attack)
        {
            bool successAttack = false;

            // Attack the center (player tile) plus up to 3 extras set in SettingAttackPlush
            // center
            if (IsValidPoint(attack.attackPointNumber_x, attack.attackPointNumber_y))
            {
                var ents = field[attack.attackPointNumber_x][attack.attackPointNumber_y].GetEntities();
                foreach (Entity entity in ents)
                {
                    if (entity.TryGetComponent<PlayerCharacter>(out var player))
                    {
                        if (player is ITurnPlayer action)
                        {
                            action.PlayerGetDamage(damage);
                            successAttack = true;
                        }
                    }
                }
            }

            // extras
            if (attack.attackPointNumberExtra_x != null)
            {
                for (int i = 0; i < attack.attackPointNumberExtra_x.Length; i++)
                {
                    int ex = attack.attackPointNumberExtra_x[i];
                    int ey = attack.attackPointNumberExtra_y[i];
                    if (!IsValidPoint(ex, ey)) continue;
                    var ents2 = field[ex][ey].GetEntities();
                    foreach (Entity entity in ents2)
                    {
                        if (entity.TryGetComponent<PlayerCharacter>(out var player))
                        {
                            if (player is ITurnPlayer action)
                            {
                                action.PlayerGetDamage(damage);
                                successAttack = true;
                            }
                        }
                    }
                }
            }

            return successAttack;
        }

        private bool AttackFourCard(Attack attack)
        {
            bool successAttack = false;

            // center
            if (IsValidPoint(attack.attackPointNumber_x, attack.attackPointNumber_y))
            {
                var ents = field[attack.attackPointNumber_x][attack.attackPointNumber_y].GetEntities();
                foreach (Entity entity in ents)
                {
                    if (entity.TryGetComponent<PlayerCharacter>(out var player))
                    {
                        if (player is ITurnPlayer action)
                        {
                            action.PlayerGetDamage(damage);
                            successAttack = true;
                        }
                    }
                }
            }

            // extras (up to 4)
            if (attack.attackPointNumberExtra_x != null)
            {
                for (int i = 0; i < attack.attackPointNumberExtra_x.Length; i++)
                {
                    int ex = attack.attackPointNumberExtra_x[i];
                    int ey = attack.attackPointNumberExtra_y[i];
                    if (!IsValidPoint(ex, ey)) continue;
                    var ents2 = field[ex][ey].GetEntities();
                    foreach (Entity entity in ents2)
                    {
                        if (entity.TryGetComponent<PlayerCharacter>(out var player))
                        {
                            if (player is ITurnPlayer action)
                            {
                                action.PlayerGetDamage(damage);
                                successAttack = true;
                            }
                        }
                    }
                }
            }

            return successAttack;
        }

        private bool AttackStraightPlush(Attack attack)
        {
            bool successAttack = false;

            List<(int x, int y)> corners = new List<(int, int)> { (0, 0), (0, 2), (2, 0), (2, 2) };
            int excludeIndex = attack.excludedCornerIndex;

            // 안전장치: 저장되어 있지 않다면 랜덤으로 선택
            if (excludeIndex < 0 || excludeIndex >= corners.Count)
            {
                excludeIndex = UnityEngine.Random.Range(0, corners.Count);
            }
            var excludedCorner = corners[excludeIndex];
            int excludedX = excludedCorner.x;
            int excludedY = excludedCorner.y;

            for (int x = 0; x < 3; x++)
            {
                for (int y = 0; y < 3; y++)
                {
                    if (x == excludedX && y == excludedY) continue;

                    var entities = field[x][y].GetEntities();
                    foreach (Entity entity in entities)
                    {
                        if (entity.TryGetComponent<PlayerCharacter>(out var player))
                        {
                            if (player is ITurnPlayer action)
                            {
                                action.PlayerGetDamage(damage);
                                successAttack = true;
                            }
                        }
                    }
                }
            }

            return successAttack;
        }

        #endregion

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
            await UniTask.Delay(1200);


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

        public void CreateAttack(bool firstCreate = false)
        {
            Attack tmpAttack = new Attack();

            tmpAttack.currentAttackStyle = SetAttackType(); // 무슨공격인지 설정 

            tmpAttack.SetAttackCycle(attackCreateCycle);

            if (firstCreate)
            {
                tmpAttack.attackTurnOrder++;
                tmpAttack.attackTurnOrder += delayAttackByRelic;

            }
            SetAttack(tmpAttack, isPlayerAttack);
            attackLists.Add(tmpAttack); // 리스트에 어택추가
        }
        public void AttackEnemyTurnStart()
        {
            EnemyTurnClear();
            LogEx.Log("Enemy Turn!!");
            AttackEnemyAwake(); // Enemy Awake시 실행되는 함수

            // 우선 시작할때 공격구역 설정하도록 해보기 Test


            AttackPatternEnemyTurning(); // Enemy가 수행하는 공격들의 TurnOrder 감소 후 공격

            AttackEnemyTurnEnd(); // Enemy 공격의 마무리 단계
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
                if (attack.attackTurnOrder == 0) // 0 이라면 공격시행
                {
                    AttackingCheck(attack);
                    RemoveHighLight(attack);
                }
            }

        }

        virtual public void AttackingCheck(Attack attack) // 공격이 성공했는지 체크
        {
            if (AttackGo(attack))
            {
                // 공격에 성공했음
                LogEx.Log("Enemy가 공격에 성공했다!");
                isAttackSuccess = true;
            }
            else
            {
                isAttackSuccess = false;
                LogEx.Log("Enemy가 공격에 실패했다!");
                //공격에 실패했음.
            }
        }



        /// <summary>
        /// 가로세로줄 공격에 성공하면  true
        /// </summary>
        /// <param name="attack"></param>
        /// <returns></returns>
        public bool AttackGo(Attack attack)
        {
            switch (attack.currentAttackStyle)
            {
                case AttackStyle.HighCard:
                    return (AttackHighCard(attack));
                case AttackStyle.OnePair:
                    return (AttackOnePare(attack));
                case AttackStyle.TwoPair:
                    return (AttackTwoPare(attack));
                case AttackStyle.Triple:
                    return (AttackTriple(attack));
                case AttackStyle.Straight:
                    return (AttackStraight(attack));
                case AttackStyle.Flush:
                    return (AttackPlush(attack));
                case AttackStyle.FourCard:
                    return (AttackFourCard(attack));
                case AttackStyle.StraightPlush:
                    return (AttackStraightPlush(attack));

                default: break;
            }

            if (attack.currentAttackStyle == AttackStyle.AttackHorizontal) // 어택 타입에 따라 행동
            {
                return (AttackHorizontal(attack.attackLineNumber));
            }
            else if (attack.currentAttackStyle == AttackStyle.AttackVertical)
            {
                return (AttackVerical(attack.attackLineNumber));
            }
            else if (attack.currentAttackStyle == AttackStyle.AttackPoint)
            {
                AttackPoint(attack.attackPointNumber_x, attack.attackPointNumber_y);
            }
            return false;
        }
        void AttackEnemyTurnEnd()
        {
            List<Attack> tmpAttacks = new List<Attack>();
            int count = 0;
            foreach (Attack attack in attackLists)
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

            for (int i = 0; i < count; i++) // 지워진 어택 갯수만큼 새로 생성
            {
                LogEx.Log("지워진 Attack 만큼 새로 생성");
                CreateAttack();
            }

            if (orderSettingGo == true)
            {
                SetAllAttackOrderGo();
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
        virtual public void SetAttack(Attack attack, bool setPlayerAttack = false)
        {
            LogEx.Log(setPlayerAttack);

            // 수정: 원래 있던 조기 return을 제거하고 attack.currentAttackStyle 에 맞게 세팅 함수 호출
            // (원본의 의도에 맞춰 각 SettingAttack*를 호출하도록 함)

            // set whether attack uses player location or random
            attack.isPlayerAttack = setPlayerAttack;

            switch (attack.currentAttackStyle)
            {

                case AttackStyle.HighCard:
                    SettingAttackHighCard(attack);
                    break;
                case AttackStyle.OnePair:
                    SettingAttackOnePare(attack);
                    break;
                case AttackStyle.TwoPair:
                    SettingAttackTwoPare(attack);
                    break;
                case AttackStyle.Triple:
                    SettingAttackTriple(attack);
                    break;
                case AttackStyle.Straight:
                    SettingAttackStraight(attack);
                    break;
                case AttackStyle.Flush:
                    SettingAttackPlush(attack);
                    break;
                case AttackStyle.FourCard:
                    SettingAttackFourCard(attack);
                    break;
                case AttackStyle.StraightPlush:
                    SettingAttackStraightPlush(attack);
                    break;
                default:
                    // fallback to highcard
                    SettingAttackHighCard(attack);
                    break;
            }
        }
        /// <summary>
        /// 플레이어위치로 공격설정
        /// </summary>
        /// <param name="attack"></param>
        public void SetPlayerAttack(Attack attack)
        {
            LogEx.Log("SetPlayerAttack!");
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
            LogEx.Log("SetRandomAttack!");
            attack.attackLineNumber = UnityEngine.Random.Range(0, 3); // 랜덤으로 위치 지정 (0..2)
        }
        AttackStyle SetAttackType()
        {
            currentAttackStyle = GetRandomAttackStyle(); // 랜덤으로 어택스타일 받기;
            return currentAttackStyle;
        }

        int nowAttackPatternIndex = 0;
        AttackStyle GetRandomAttackStyle() // 랜덤으로 어택스타일 받기
        {
            Array values = Enum.GetValues(typeof(AttackStyle));
            return baseMobBossData.AttackPattern[nowAttackPatternIndex++ % baseMobBossData.AttackPattern.Count];
            // return (isPlayerAttack) ? (AttackStyle)values.GetValue(UnityEngine.Random.Range(2, values.Length)) : (AttackStyle)values.GetValue(UnityEngine.Random.Range(1, values.Length));
            // 랜덤값 다르게받기
        }

        #region HighLight관련
        public void RemoveHighLight(Attack attack)
        {
            if (attack == null) return;

            switch (attack.currentAttackStyle)
            {
                case AttackStyle.AttackHorizontal:
                    RemoveHighLight_Horizontal(attack.attackLineNumber);
                    break;

                case AttackStyle.AttackVertical:
                    RemoveHighLight_Vertical(attack.attackLineNumber);
                    break;

                case AttackStyle.AttackPoint:
                case AttackStyle.HighCard:
                    RemoveHighLight_Point(attack.attackPointNumber_x, attack.attackPointNumber_y);
                    break;

                case AttackStyle.OnePair:
                    RemoveHighLight_Point(attack.attackPointNumber_x, attack.attackPointNumber_y);
                    if (attack.attackPointNumberExtra_x != null && attack.attackPointNumberExtra_x.Length > 0)
                        RemoveHighLight_Point(attack.attackPointNumberExtra_x[0], attack.attackPointNumberExtra_y[0]);
                    break;

                case AttackStyle.TwoPair:
                    RemoveHighLight_Point(attack.attackPointNumber_x, attack.attackPointNumber_y);
                    if (attack.attackPointNumberExtra_x != null)
                    {
                        for (int i = 0; i < attack.attackPointNumberExtra_x.Length; i++)
                            RemoveHighLight_Point(attack.attackPointNumberExtra_x[i], attack.attackPointNumberExtra_y[i]);
                    }
                    break;

                case AttackStyle.Triple:
                    if (attack.currentAttackStyle == AttackStyle.AttackHorizontal)
                        RemoveHighLight_Horizontal(attack.attackLineNumber);
                    else if (attack.currentAttackStyle == AttackStyle.AttackVertical)
                        RemoveHighLight_Vertical(attack.attackLineNumber);
                    else
                        RemoveHighLight_Point(attack.attackPointNumber_x, attack.attackPointNumber_y);
                    break;

                case AttackStyle.Straight:
                    if (attack.currentAttackStyle == AttackStyle.AttackHorizontal)
                        RemoveHighLight_Horizontal(attack.attackLineNumber);
                    else if (attack.currentAttackStyle == AttackStyle.AttackVertical)
                        RemoveHighLight_Vertical(attack.attackLineNumber);
                    else
                        RemoveHighLight_Point(attack.attackPointNumber_x, attack.attackPointNumber_y);

                    if (attack.attackPointNumberExtra_x != null)
                    {
                        for (int i = 0; i < attack.attackPointNumberExtra_x.Length; i++)
                            RemoveHighLight_Point(attack.attackPointNumberExtra_x[i], attack.attackPointNumberExtra_y[i]);
                    }
                    break;

                case AttackStyle.Flush:
                    RemoveHighLight_Point(attack.attackPointNumber_x, attack.attackPointNumber_y);
                    if (attack.attackPointNumberExtra_x != null)
                    {
                        for (int i = 0; i < attack.attackPointNumberExtra_x.Length; i++)
                            RemoveHighLight_Point(attack.attackPointNumberExtra_x[i], attack.attackPointNumberExtra_y[i]);
                    }
                    break;

                case AttackStyle.FourCard:
                    RemoveHighLight_Point(attack.attackPointNumber_x, attack.attackPointNumber_y);
                    if (attack.attackPointNumberExtra_x != null)
                    {
                        for (int i = 0; i < attack.attackPointNumberExtra_x.Length; i++)
                            RemoveHighLight_Point(attack.attackPointNumberExtra_x[i], attack.attackPointNumberExtra_y[i]);
                    }
                    break;

                case AttackStyle.StraightPlush:
                    // 세팅 시 저장한 excludedCornerIndex를 참조해 그 칸만 스킵하고 나머지 타일의 하이라이트를 끔
                    List<(int x, int y)> cornersSP = new List<(int, int)> { (0, 0), (0, 2), (2, 0), (2, 2) };
                    if (attack.excludedCornerIndex >= 0 && attack.excludedCornerIndex < cornersSP.Count)
                    {
                        var excludedCorner = cornersSP[attack.excludedCornerIndex];
                        int excludedX = excludedCorner.x;
                        int excludedY = excludedCorner.y;

                        for (int x = 0; x < 3; x++)
                        {
                            for (int y = 0; y < 3; y++)
                            {
                                if (x == excludedX && y == excludedY) continue;
                                field[x][y].Unhightlight(Define.HighlightType.DefaultAttack);
                            }
                        }
                    }
                    else
                    {
                        // 안전장치: 만약 저장값이 없다면 전체 해제
                        for (int x = 0; x < 3; x++)
                            for (int y = 0; y < 3; y++)
                                field[x][y].Unhightlight(Define.HighlightType.DefaultAttack);
                    }
                    break;

                default:
                    // 안전하게 메인 포인트 + extras 해제
                    RemoveHighLight_Point(attack.attackPointNumber_x, attack.attackPointNumber_y);
                    if (attack.attackPointNumberExtra_x != null)
                    {
                        for (int i = 0; i < attack.attackPointNumberExtra_x.Length; i++)
                            RemoveHighLight_Point(attack.attackPointNumberExtra_x[i], attack.attackPointNumberExtra_y[i]);
                    }
                    break;
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

        // 실질적인 공격 후 데미지 주기
        void AttackPoint(int pointNumber_x, int poinNumber_y)
        {
            if (!IsValidPoint(pointNumber_x, poinNumber_y)) return;

            List<Entity> entities =
            field[pointNumber_x][poinNumber_y].GetEntities(); // 찾아보는 타일에 있는 Entity 받아오기

            //Entity중 Player가 있다면
            foreach (Entity entity in entities)
            {
                if (entity.TryGetComponent<PlayerCharacter>(out var player)) // 존재하는걸 확인했다면
                {
                    // PlayerCharacter가 ITurnPlayerAction을 구현중임.
                    if (player is ITurnPlayer action)
                    {
                        action.PlayerGetDamage(damage);
                    }
                }
            }

        }

        bool AttackVerical(int pointNumber) // 세로 공격 왼쪽부터 pointNumber 0,1,2
        {
            // pointNuber가 이상하다.
            bool successAttack = false;
            // 가로는 0,1,2 모두
            for (int x = 0; x < 3; x++)
            {
                List<Entity> entities =
                field[x][pointNumber].GetEntities(); // 찾아보는 타일에 있는 Entity 받아오기

                //Entity중 Player가 있다면
                foreach (Entity entity in entities)
                {
                    if (entity.TryGetComponent<PlayerCharacter>(out var player)) // 존재하는걸 확인했다면
                    {
                        // PlayerCharacter가 ITurnPlayerAction을 구현중임.
                        if (player is ITurnPlayer action)
                        {
                            action.PlayerGetDamage(damage);
                            successAttack = true;
                        }
                    }
                }

            }

            return successAttack;
        }


        bool AttackHorizontal(int pointNumber) // 가로 공격 왼쪽부터 pointNumber 0,1,2
        {
            bool successAttack = false;
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
                        if (player is ITurnPlayer action)
                        {
                            action.PlayerGetDamage(damage);
                            successAttack = true;
                        }
                    }
                }

            }
            return successAttack;
        }
        #endregion


        #region Player 상호작용 
        public virtual bool GetDamage(float damage)
        {
            HP -= damage;
            LogEx.Log($"{damage}만큼의 피해를 입러 HP가 {HP}로 감소하였다!");
            UpdateHPBar();
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


        #endregion

        #region Tool 관련

        private void EnemyTurnClear()
        {
            isAttackSuccess = false;
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

        public void TurnClear()
        {
            isAttakced = false;
        }
        public void IsAttacked(int amount)
        {
            isAttakced = true;
            GetDamage(amount);
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
        public void Setup(BaseMobBossData _baseMobBossData)
        {
            baseMobBossData = _baseMobBossData;
            Debug.Log($"SetUp! : {_baseMobBossData.MobID}");

            if (baseMobBossData.BoolAttackType) // 1 이라면
            {
                attackCreateCycle = _baseMobBossData.AttackCycle;
            }
            else
            {
                SetPatternRandomBaseWeight();
            }
            isPlayerAttack = baseMobBossData.AttackPlayer;

            SettingGimmick(_baseMobBossData);

        }
            
        private void SettingGimmick(BaseMobBossData baseMobBossData)
        {
            // TODO : 여러개 기믹이 존재하는 몹이 있으면 [0]을 수정해야함.

            char trimText = '"';
            IGimmick igimmick = GimmickFactory.Instance.CreateGimmick(baseMobBossData.GimmickName[0].ToString().Trim(trimText));
            igimmick.Apply(this);
        }
        private void SetPatternRandomBaseWeight()
        {
            List<int> weights = baseMobBossData.AttackWeight;

            List<AttackStyle> originalPatterns = baseMobBossData.AttackPattern;

            Debug.Log("11");
            // 1. 유효성 검사
            if (weights == null || originalPatterns == null || weights.Count == 0 || weights.Count != originalPatterns.Count)
            {
                Debug.LogError("Enemy Setup: AttackWeight or AttackPattern data is invalid or mismatched.");
                return;
            }

                // int totalWeight = weights.Sum(); // Linq 사용 시
                int totalWeight = 0;
                foreach (int w in weights)
                {
                    totalWeight += w;
                }

                // 총 가중치가 0 이하면 로직 실행 불가
                if (totalWeight <= 0)
                {
                    Debug.LogError("Enemy Setup: Total weight is 0. Cannot generate weighted pattern.");
                    return;
                }
               

                // 15개의 새로운 패턴을 저장할 리스트 생성
                // (만약 originalPatterns가 List<string>이면 여기도 List<string>으로 변경)
                List<AttackStyle> newGeneratedPattern = new List<AttackStyle>(15);

                // 15번 반복하여 패턴 생성
                for (int i = 0; i < 15; i++)
                {
                    // 0 ~ (totalWeight - 1) 사이의 랜덤 값 생성
                    int randomValue = UnityEngine.Random.Range(0, totalWeight);

                    int currentWeightSum = 0;
                    // 5. 가중치 리스트를 순회하며 랜덤 값이 어느 구간에 속하는지 확인
                    for (int j = 0; j < weights.Count; j++)
                    {
                        currentWeightSum += weights[j];

                        // 6. 랜덤 값이 현재 가중치 누적 합보다 작으면, 이 패턴(j)을 선택
                        // 예: weight[5, 2, 3], total=10
                        // randomValue=0~4 (5개) -> j=0 선택
                        // randomValue=5~6 (2개) -> j=1 선택
                        // randomValue=7~9 (3개) -> j=2 선택
                        if (randomValue < currentWeightSum)
                        {
                            // 해당 인덱스(j)의 "원본 패턴"을 새 리스트에 추가
                            newGeneratedPattern.Add(originalPatterns[j]);

                            // 이(i) 번째 패턴을 찾았으므로 j루프 탈출
                            break;
                        }
                    }
                }

                // 7. "baseMobBossData에 저장"
                // -> 생성된 15개의 패턴 리스트로 기존 AttackPattern 리스트를 덮어씁니다.
                baseMobBossData.AttackPattern = newGeneratedPattern;


                Debug.Log("여기 진입 완료");
                for(int x=0;x<15;x++)
                {
                    Debug.Log($"패턴 리스트 : {baseMobBossData.AttackPattern[x]}");
                }
                // --- 구현 종료 ---
            }
            #endregion
        }
    }

