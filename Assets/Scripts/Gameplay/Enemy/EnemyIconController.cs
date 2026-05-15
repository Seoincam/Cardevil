using UnityEngine;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using Database.Generated;
using DG.Tweening;

namespace Cardevil.Gameplay.Enemy
{
    // IconController를 상속받아 AddIcon 등의 기능을 그대로 물려받습니다.
    public class EnemyIconController : IconController
    {
        [Header("Enemy Default Sprite Data")]
        [SerializeField] private Sprite _defaultGimmickSprite;
        [SerializeField] private List<Sprite> _adIconSprites;
        [SerializeField] private List<Sprite> _actDelayIconSprites;

        // Inspector에서 할당하지 않고, 코드 내부에서 생성 후 캐싱(저장)합니다.
        private IconUIElement _infoIconElement;
        private IconUIElement _adIconElement;
        private IconUIElement _delayIconElement;

        private BaseMobBossData _bossData;

        /// <summary>
        /// 1) 몬스터 데이터 주입 시 3개의 기본 아이콘을 초기화합니다.
        /// </summary>
        public void SetMonsterInfo(BaseMobBossData infoData)
        {
            _bossData = infoData;
            if (_bossData == null) return;

            // 이미 생성되어 있다면 생성하지 않음 (재사용 방어코드)
            if (_infoIconElement == null)
            {
                // 부모의 AddIcon 기능을 이용해 아이콘을 만들고 변수에 저장합니다.
                _infoIconElement = AddIcon(_defaultGimmickSprite, "기믹 정보", _bossData.GimmickName[0]);

                // 공격력과 딜레이 아이콘도 초기 상태(Index 0)로 띄워둡니다.
                _adIconElement = AddIcon(_adIconSprites.Count > 0 ? _adIconSprites[0] : null, "현재 공격력", "위협 수준: 측정 불가");
                _delayIconElement = AddIcon(_actDelayIconSprites.Count > 0 ? _actDelayIconSprites[0] : null, "공격 주기 정보", "주기: 계산 중");
            }
            else
            {
                // 이미 존재한다면 데이터만 갱신
                _infoIconElement.UpdateTooltipData("기믹 정보", _bossData.GimmickName[0]);
            }
        }

        /// <summary>
        /// 2) 공격력 갱신
        /// </summary>
        public void UpdateAttack(int attackPower)
        {
            if (_adIconElement == null) return;

            int index = Mathf.Clamp(attackPower - 1, 0, _adIconSprites.Count - 1);
            _adIconElement.SetSprite(_adIconSprites[index]);
            _adIconElement.UpdateTooltipData("현재 공격력", $"위협 수준: {attackPower}");
        }

        /// <summary>
        /// 3) 턴 감소 시 쿨타임(모래시계) 갱신
        /// </summary>
        public async UniTaskVoid UpdateDelayAsync(int currentRemainingTurn)
        {
            if (_delayIconElement == null) return;

            int index = Mathf.Clamp(currentRemainingTurn - 1, 0, _actDelayIconSprites.Count - 1);
            _delayIconElement.SetSprite(_actDelayIconSprites[index]);

            string attackCycleInfo = _bossData != null ? _bossData.AttackCycle.ToString() : "알 수 없음";
            string tooltipDesc = $"기본 공격 주기: {attackCycleInfo}\n현재 남은 턴: {currentRemainingTurn}";

            _delayIconElement.UpdateTooltipData("공격 주기 정보", tooltipDesc);

            // 흔들림 연출
            if (currentRemainingTurn <= 2 && currentRemainingTurn > 0)
            {
                _delayIconElement.PlayWarningAnimation();
            }
            else
            {
                _delayIconElement.StopAnimation();
            }

            // 반짝임 연출
            _delayIconElement.GetComponent<UnityEngine.UI.Image>().DOColor(Color.red, 0.2f).SetLoops(2, LoopType.Yoyo);
            await UniTask.Delay(TimeSpan.FromSeconds(0.4f));
        }
    }
}