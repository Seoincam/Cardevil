using UnityEngine;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;


namespace Cardevil.Gameplay.Enemy
{
    public class AttackCardAnimation : MonoBehaviour
    {
        [SerializeField] private List<Image> _cardImage;

        [SerializeField] private TMP_Text _cardText;

        [Header("Animation Settings")]
        [SerializeField] private float _spreadAngle = 15f; // 카드가 기울어지는 각도
        [SerializeField] private float _spreadDistanceX = 40f; // 카드가 좌우로 퍼지는 거리
        [SerializeField] private float _spreadDropY = 5f; // 가장자리 카드가 아래로 내려가는 정도 (원형 느낌)

        public async UniTask AttackAnimationStart(AttackStyle attackStyle)
        {
            // 텍스트 초기화
            _cardText.color = new Color(_cardText.color.r, _cardText.color.g, _cardText.color.b, 0f);

            var fadeTasks = new List<UniTask>();

            // 1. 카드들의 초기 이미지 세팅 및 페이드 인
            for (int i = 0; i < _cardImage.Count; i++)
            {
                RectTransform rect = _cardImage[i].rectTransform;

                // 각 카드들은 PosX 5의 간격으로 0부터 배열 (0, 5, 10, 15, 20)
                rect.anchoredPosition = new Vector2(i * 5f, 0f);
                rect.localRotation = Quaternion.identity;

                // 알파값 0으로 초기화
                Color color = _cardImage[i].color;
                color.a = 0f;
                _cardImage[i].color = color;

                // 페이드 인 애니메이션 (0.5초)
                fadeTasks.Add(_cardImage[i].DOFade(1f, 0.5f).ToUniTask());
            }

            // 모든 카드가 나타날 때까지 대기
            await UniTask.WhenAll(fadeTasks);

            // 2. 카드 스프레드 애니메이션 시작 (매개변수로 attackStyle 전달)
            await CardSpreadAnimation(attackStyle);
        }

        private async UniTask CardSpreadAnimation(AttackStyle attackStyle)
        {
            var spreadTasks = new List<UniTask>();

            // 카드의 중앙 인덱스 계산 (5장일 경우 2)
            float middleIndex = (_cardImage.Count - 1) / 2f;

            for (int i = 0; i < _cardImage.Count; i++)
            {
                RectTransform rect = _cardImage[i].rectTransform;

                // 중앙을 기준으로 한 상대적 위치 (-2, -1, 0, 1, 2)
                float offset = i - middleIndex;

                // 타겟 각도: 왼쪽 카드는 양수(왼쪽 기울임), 오른쪽 카드는 음수(오른쪽 기울임)
                float targetAngle = -offset * _spreadAngle;

                // 타겟 위치: X축은 좌우 퍼짐, Y축은 중앙에서 멀어질수록 아래로 떨어지게 (2차 함수 형태 적용)
                float targetX = offset * _spreadDistanceX;
                float targetY = -(offset * offset) * _spreadDropY;

                // 위치와 회전 애니메이션 동시에 실행 (OutBack 이즈를 주어 카드가 튕기듯 펼쳐지는 효과)
                spreadTasks.Add(rect.DOAnchorPos(new Vector2(targetX, targetY), 0.6f).SetEase(Ease.OutBack).ToUniTask());
                spreadTasks.Add(rect.DORotate(new Vector3(0, 0, targetAngle), 0.6f).SetEase(Ease.OutBack).ToUniTask());
            }

            await UniTask.WhenAll(spreadTasks);

            // 3. 카드 활성화 및 텍스트 표시
            _cardText.text = attackStyle.ToString(); // 족보 이름 세팅
            var textFadeTask = _cardText.DOFade(1f, 0.5f).ToUniTask();
            var specialActionTask = CardSpecialAction(attackStyle);

            await UniTask.WhenAll(textFadeTask, specialActionTask);

            // 사용자가 결과를 확인할 수 있도록 잠시 대기
            await UniTask.Delay(TimeSpan.FromSeconds(1.5f));

            // 4. 모든 애니메이션 종료 후 FadeOut 및 원위치
            var fadeOutTasks = new List<UniTask>();
            foreach (var img in _cardImage)
            {
                fadeOutTasks.Add(img.DOFade(0f, 0.5f).ToUniTask());
            }
            fadeOutTasks.Add(_cardText.DOFade(0f, 0.5f).ToUniTask());

            await UniTask.WhenAll(fadeOutTasks);

            // 5. 원위치 리셋 (다음 공격을 위해)
            for (int i = 0; i < _cardImage.Count; i++)
            {
                _cardImage[i].rectTransform.anchoredPosition = new Vector2(i * 5f, 0f);
                _cardImage[i].rectTransform.localRotation = Quaternion.identity;
                _cardImage[i].color = new Color(1, 1, 1, 0); // 색상 원상복구 (알파는 0)
            }
        }

        private async UniTask CardSpecialAction(AttackStyle attackStyle)
        {
            int activeCardCount = 0;

            // AttackStyle에 따라 활성화할 카드 개수 결정
            switch (attackStyle)
            {
                case AttackStyle.HighCard: activeCardCount = 1; break;
                case AttackStyle.OnePair: activeCardCount = 2; break;
                case AttackStyle.TwoPair: activeCardCount = 4; break;
                case AttackStyle.Triple: activeCardCount = 3; break;
                case AttackStyle.Straight:
                case AttackStyle.Flush:
                case AttackStyle.StraightFlush:
                    activeCardCount = 5; break; // 5장 모두 활성화
                default: activeCardCount = 0; break;
            }

            var colorTasks = new List<UniTask>();

            for (int i = 0; i < _cardImage.Count; i++)
            {
                // 활성화될 개수에 속하면 원래 색(White), 아니면 어둡게(Gray)
                Color targetColor = (i < activeCardCount) ? Color.white : Color.gray;

                // 알파값은 1을 유지해야 보임
                targetColor.a = 1f;

                colorTasks.Add(_cardImage[i].DOColor(targetColor, 0.3f).ToUniTask());
            }

            await UniTask.WhenAll(colorTasks);
        }

    }
}
