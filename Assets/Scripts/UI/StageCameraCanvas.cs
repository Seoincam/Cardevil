using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Threading;
using UnityEngine;

namespace Cardevil.UI
{
    public class StageCameraCanvas : MonoBehaviour
    {
        private static StageCameraCanvas _instance;
        
        public static StageCameraCanvas Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindAnyObjectByType<StageCameraCanvas>();
                }
                return _instance;
            }
        }


        public RectTransform leftRock;
        public RectTransform rightRock;

        public Vector2 leftRockStartPos;
        public Vector2 rightRockStartPos;

        private void Awake()
        {
            _instance = this;
            
            leftRockStartPos = leftRock.anchoredPosition;
            rightRockStartPos = rightRock.anchoredPosition;
            InitRock();
        }

        
        public void InitRock()
        {
            leftRock.gameObject.SetActive(false);
            rightRock.gameObject.SetActive(false);
        }

        public async UniTask AnimHideRock(float duration, Ease ease = Ease.OutCubic, CancellationToken ct = default)
        {
            leftRock.gameObject.SetActive(true);
            rightRock.gameObject.SetActive(true);
            leftRock.anchoredPosition = leftRockStartPos;
            rightRock.anchoredPosition = rightRockStartPos;
            float leftTargetX = leftRockStartPos.x - leftRock.rect.width;
            float rightTargetX = rightRockStartPos.x + rightRock.rect.width;

            var leftTween = leftRock.DOAnchorPosX(leftTargetX, duration).SetEase(ease);
            var rightTween = rightRock.DOAnchorPosX(rightTargetX, duration).SetEase(ease);
            
            await UniTask.WhenAll(
                leftTween.ToUniTask(cancellationToken: ct),
                rightTween.ToUniTask(cancellationToken: ct));
            leftRock.gameObject.SetActive(false);
            rightRock.gameObject.SetActive(false);
        }

        public async UniTask AnimShowRock(float duration, Ease ease = Ease.OutCubic, CancellationToken ct = default)
        {
            leftRock.gameObject.SetActive(true);
            rightRock.gameObject.SetActive(true);
            
            leftRock.anchoredPosition = new Vector2(leftRockStartPos.x - leftRock.rect.width, leftRockStartPos.y);
            rightRock.anchoredPosition = new Vector2(rightRockStartPos.x + rightRock.rect.width, rightRockStartPos.y);
            
            var leftTween = leftRock.DOAnchorPosX(leftRockStartPos.x, duration).SetEase(ease);
            var rightTween = rightRock.DOAnchorPosX(rightRockStartPos.x, duration).SetEase(ease);
            

            await UniTask.WhenAll(
                leftTween.ToUniTask(cancellationToken: ct),
                rightTween.ToUniTask(cancellationToken: ct)
                );
        }
    }
}