using UnityEngine;
using Cysharp.Threading.Tasks;
using DG.Tweening;

namespace Cardevil.Gameplay.Enemy
{
    public class EnemyAnimationController : MonoBehaviour
    {
        [Header("Connects")]
        [SerializeField] Enemy _enemy;
        [SerializeField] SpriteRenderer _enemySpriteRenderer;
        [SerializeField] Transform _enemyTransform;

        [Header("Settings")]
        [SerializeField] float _attacked_BackPosition = 2f;
        [SerializeField] float _attack_BackPosition = 2f;
        [SerializeField] float _attack_FrontPosition = 1.5f;

        // 공격 애니메이션: 뒤로 살짝 빠졌다가 앞으로 돌진 후 원위치
        public async UniTask EnemyAttackAnimation()
        {
            Debug.Log("enemyAttackAnimation 실행");

            Vector3 originPos = _enemyTransform.position;
            // 적의 '오른쪽' 방향 벡터를 가져와서 사용
            Vector3 rightDir = _enemyTransform.right;
            rightDir.y = 0f; // 공중으로 뜨거나 땅으로 꺼지지 않도록 Y축 고정
            rightDir.Normalize();

            // 1. 공격(돌진): 화면 왼쪽 중간으로 찌르기 (-rightDir)
            Vector3 attackOffset = -rightDir * _attack_FrontPosition;
            // 2. 준비(뒤로 빠짐): 화면 오른쪽 뒤로 빠지기 (rightDir)
            Vector3 readyOffset = rightDir * _attack_BackPosition;

            Sequence attackSeq = DOTween.Sequence();

            // 뒤로 살짝 빼기
            attackSeq.Append(_enemyTransform.DOMove(originPos + readyOffset, 0.4f).SetEase(Ease.OutQuad));
            // 앞으로 훅 찌르기
            attackSeq.Append(_enemyTransform.DOMove(originPos + attackOffset, 0.15f).SetEase(Ease.OutFlash));

            // 돌진이 끝날 때까지 대기
            await attackSeq.AsyncWaitForCompletion();

            // 복귀 애니메이션
            _enemyTransform.DOMove(originPos, 0.2f).SetEase(Ease.InOutSine);
        }

        // 피격 애니메이션: 빨갛게 번쩍이면서 살짝 흔들림
        public async UniTask EnemyAttackedAnimation()
        {
            Vector3 originPos = _enemyTransform.position;
            Vector3 knockbackOffset = new Vector3(_attacked_BackPosition, 0f, 0f);

            Sequence seq = DOTween.Sequence();

            // 1. 피격 순간
            seq.Append(_enemySpriteRenderer.DOColor(Color.red, 0.05f));
            seq.Join(_enemyTransform.DOMove(originPos + knockbackOffset, 0.2f).SetEase(Ease.OutQuad));
            seq.Join(_enemyTransform.DOShakePosition(0.1f, strength: new Vector3(0.2f, 0.2f, 0f), vibrato: 10, randomness: 90));

            // 2. 복귀
            seq.Append(_enemySpriteRenderer.DOColor(Color.white, 0.2f));
            seq.Join(_enemyTransform.DOMove(originPos, 0.35f).SetEase(Ease.InOutSine));

            await seq.AsyncWaitForCompletion();
        }

        // 사망 애니메이션: 투명해지면서 옆으로 쓰러짐
        public async UniTask EnemyDeathAnimation()
        {
            Sequence seq = DOTween.Sequence();
            // 기존 회전값(10, 60, 1)에서 Z축을 90도로 꺾어 옆으로 쓰러지는 연출
            Vector3 fallRotation = new Vector3(10f, 60f, 90f);

            // 투명해지기(FadeOut) & 옆으로 쓰러지기
            seq.Append(_enemySpriteRenderer.DOFade(0f, 0.5f).SetEase(Ease.OutQuad));
            seq.Join(_enemyTransform.DORotate(fallRotation, 0.5f).SetEase(Ease.InBack));

            await seq.AsyncWaitForCompletion();
        }

        public async UniTask EnemyStartAnimation()
        {
            // Enemy Transform 5.64,-0.89
            _enemySpriteRenderer.sprite = Resources.Load<Sprite>("Enemy/Slave_Yellow");
            // 이미지 기준 Enemy Transform 할당 
            // Position (6, -2.6, 1)
            _enemyTransform.position = new Vector3(6f, -2.6f, 1f);

            // Rotation (10, 60, 1) 
            // (참고: 이미지상 Z값이 1로 입력되어 있어 그대로 적용했습니다. 필요시 0으로 수정하세요)
            _enemyTransform.rotation = Quaternion.Euler(10f, 60f, 1f);

            // Scale (0.6, 0.6, 0.6)
            _enemyTransform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
        }
    }
}