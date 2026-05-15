using UnityEngine;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using Database.Generated;
using DG.Tweening;
using System;


namespace Cardevil.Gameplay.Enemy
{
    public class IconController : MonoBehaviour
    {
        [Header("Sprite Data")]
        [SerializeField] private Sprite _defaultGimmickSprite;
        [SerializeField] private List<Sprite> _adIconSprites;
        [SerializeField] private List<Sprite> _actDelayIconSprites;

        [Header("UI Element References")]
        [SerializeField] private IconUIElement _infoIconElement;
        [SerializeField] private IconUIElement _adIconElement;
        [SerializeField] private IconUIElement _delayIconElement;

        private BaseMobBossData _bossData;

        // InitIcons() 함수는 이제 필요 없으므로 삭제했습니다.

        /// <summary>
        /// 1) 몬스터 데이터 주입 시 기믹 툴팁 세팅
        /// </summary>
        public void SetMonsterInfo(BaseMobBossData infoData)
        {
            _bossData = infoData;

            if (_bossData == null) return;

            // 기믹 아이콘 설정
            _infoIconElement.SetSprite(_defaultGimmickSprite);

            // 핵심: 기믹 툴팁 데이터 주입
            _infoIconElement.UpdateTooltipData("기믹 정보", _bossData.GimmickName[0]);
        }

        /// <summary>
        /// 2) 공격력 갱신
        /// </summary>
        public void UpdateAttack(int attackPower)
        {
            int index = Mathf.Clamp(attackPower - 1, 0, _adIconSprites.Count - 1);
            _adIconElement.SetSprite(_adIconSprites[index]);

            // 공격력 툴팁 갱신
            _adIconElement.UpdateTooltipData("현재 공격력", $"위협 수준: {attackPower}");
        }

        /// <summary>
        /// 3) 턴 감소 시 쿨타임(모래시계) 갱신
        /// </summary>
        public async UniTaskVoid UpdateDelayAsync(int currentRemainingTurn)
        {
            int index = Mathf.Clamp(currentRemainingTurn - 1, 0, _actDelayIconSprites.Count - 1);
            _delayIconElement.SetSprite(_actDelayIconSprites[index]);

            // 핵심: 남은 턴 수가 바뀔 때마다 툴팁 내용을 새로 덮어씌웁니다.
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

            // 반짝임 연출 (선택사항)
            _delayIconElement.GetComponent<UnityEngine.UI.Image>().DOColor(Color.red, 0.2f).SetLoops(2, DG.Tweening.LoopType.Yoyo);
            await UniTask.Delay(TimeSpan.FromSeconds(0.4f));
        }
    }
}
        