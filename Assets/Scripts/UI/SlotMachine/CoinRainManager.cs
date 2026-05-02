using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Cysharp.Threading.Tasks;

namespace Cardevil.UI.VFX
{
    /// <summary>
    /// 잭팟 시 화면 상단에서 동전 이미지가 비처럼 쏟아지는 연출을 관리하는 클래스
    /// </summary>
    public class CoinRainManager : MonoBehaviour
    {
        [Header("Connects")]
        [SerializeField] private GameObject _coinPrefab; // [필수] 동전 이미지 프리팹
        [SerializeField] private RectTransform _rainContainer; // [필수] 동전들이 생성될 UI 컨테이너 (화면 전체 크기 권장)
        [SerializeField] private RectTransform _slotMachineRect;

        [Header("Rain Settings")]
        [SerializeField] private int _poolCount = 100; // 미리 생성해둘 동전 개수
        [SerializeField] private float _spawnDelay = 0.05f; // 동전 생성 간격 (낮을수록 마구 쏟아짐)
        [SerializeField] private float _fallDurationMin = 0.8f; // 떨어지는 시간 최소값
        [SerializeField] private float _fallDurationMax = 1.3f; // 떨어지는 시간 최대값
        [SerializeField] private float _fallDistance = 1500f; // 떨어질 거리 (화면 세로 크기보다 크게 설정)

        // 오브젝트 풀
        private Queue<RectTransform> _coinPool = new Queue<RectTransform>();

        private float _containerWidth;
        private float _spawnY;

        private void Awake()
        {
            InitializePool();
        }

        private void Start()
        {
            // 생성 위치 계산을 위해 컨테이너의 너비를 가져옴
            _containerWidth = _rainContainer.rect.width;

            // 컨테이너 중앙 상단을 생성 Y좌표로 잡음
            _spawnY = _rainContainer.rect.height * 0.5f + 100f; // 화면 밖 위쪽 100px
        }

        /// <summary>
        /// 동전 오브젝트 풀을 초기화합니다. (게임 시작 시 미리 생성)
        /// </summary>
        private void InitializePool()
        {
            if (_coinPrefab == null || _rainContainer == null)
            {
                Debug.LogError("[CoinRain] 프리팹이나 컨테이너가 할당되지 않았습니다.");
                return;
            }

            for (int i = 0; i < _poolCount; i++)
            {
                GameObject coin = Instantiate(_coinPrefab, _rainContainer);
                RectTransform rt = coin.GetComponent<RectTransform>();

                // 초기 상태 설정
                coin.SetActive(false);
                _coinPool.Enqueue(rt);
            }
        }

        /// <summary>
        /// 외부에서 호출할 잭팟 연출 시작 함수
        /// </summary>
        /// <param name="duration">연출 지속 시간 (초)</param>
        public void PlayJackpotEffect(float duration = 3.0f)
        {
            // 비동기로 코인 비 소나기 실행
            _slotMachineRect.DOShakePosition(duration, strength: 20f, vibrato: 30, randomness: 90f, snapping: false, fadeOut: true);
            SpawnCoinRainLoop(duration).Forget();
        }

        /// <summary>
        /// 지속 시간 동안 동전을 지속적으로 생성하여 떨어뜨리는 비동기 루프
        /// </summary>
        private async UniTaskVoid SpawnCoinRainLoop(float playDuration)
        {
            float timer = 0f;

            Debug.Log("[CoinRain] 코인 소나기 연출 시작!");

            while (timer < playDuration)
            {
                // 풀에서 동전 가져오기
                RectTransform coin = GetCoinFromPool();
                if (coin != null)
                {
                    // 떨어뜨리는 연출 실행
                    FallCoin(coin).Forget();
                }

                // 생성 간격만큼 대기 (Realtime 기준)
                await UniTask.Delay(System.TimeSpan.FromSeconds(_spawnDelay), ignoreTimeScale: true, cancellationToken: this.GetCancellationTokenOnDestroy());
                timer += _spawnDelay;
            }

            Debug.Log("[CoinRain] 코인 소나기 생성 종료.");
        }

        private RectTransform GetCoinFromPool()
        {
            if (_coinPool.Count > 0)
            {
                RectTransform coin = _coinPool.Dequeue();
                coin.gameObject.SetActive(true);
                return coin;
            }
            // 풀이 모자라면 강제로 리턴하지 않거나, 새로 Instantiate 하도록 구현할 수 있음 (여기선 리턴 안 함)
            return null;
        }

        private void ReturnCoinToPool(RectTransform coin)
        {
            coin.DOKill(); // 혹시 남은 트윈 Kill
            coin.gameObject.SetActive(false);
            _coinPool.Enqueue(coin);
        }

        /// <summary>
        /// 개별 동전 하나에 대한 낙하 및 회전 애니메이션 (DOTween)
        /// </summary>
        private async UniTaskVoid FallCoin(RectTransform coin)
        {
            // 1. 초기 위치 설정 (화면 상단 가로 랜덤 위치)
            float randomX = Random.Range(-_containerWidth * 0.5f, _containerWidth * 0.5f);
            coin.anchoredPosition = new Vector2(randomX, _spawnY);

            // 초기 회전값 랜덤
            coin.rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);

            // 초기 스케일 (살짝 랜덤하게 주면 더 입체적임)
            float randomScale = Random.Range(0.8f, 1.2f);
            coin.localScale = Vector3.one * randomScale;

            // 랜덤한 낙하 시간 설정
            float duration = Random.Range(_fallDurationMin, _fallDurationMax);

            // 2. DOTween 연출 실행
            Sequence seq = DOTween.Sequence().SetUpdate(true); // 타임스케일 영향 안 받게 설정

            // [애니메이션 A] 아래로 낙하 (Linear로 일정하게)
            seq.Join(coin.DOAnchorPosY(coin.anchoredPosition.y - _fallDistance, duration).SetEase(Ease.Linear));

            // [애니메이션 B] Y축 회전 (빙글빙글)
            float rotZ = Random.Range(360f, 720f); // 1~2바퀴
            seq.Join(coin.DORotate(new Vector3(0, coin.rotation.eulerAngles.z + rotZ, 0), duration, RotateMode.FastBeyond360).SetEase(Ease.OutSine));

            // [애니메이션 C] 떨어질 때 살짝 투명해지며 사라짐 (옵션)
            seq.Join(coin.GetComponent<Image>().DOFade(0.2f, duration).SetEase(Ease.InQuad).From(1f));

            // 3. 연출 완료 대기 후 풀로 반환
            await seq.ToUniTask(cancellationToken: this.GetCancellationTokenOnDestroy());

            ReturnCoinToPool(coin);
        }
    }
}