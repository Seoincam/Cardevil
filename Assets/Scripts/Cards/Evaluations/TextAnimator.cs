using Cysharp.Threading.Tasks;
using System.Threading;
using TMPro;
using UnityEngine;

namespace Cardevil.Cards.Evaluations
{
    [RequireComponent(typeof(TextMeshProUGUI), typeof(RectTransform))]
    public class TextAnimator : MonoBehaviour
    {
        [SerializeField] bool isWave = false;
        [SerializeField] float waveSpeed = 2f;
        [SerializeField] float waveLength = .1f;
        [Space]
        [SerializeField] bool shakeX = true;
        [SerializeField] bool shakeY = true;
        [Space]
        [SerializeField] float shakePower = 10f;
        [SerializeField] float shakeFrequency = 2f;

        private CancellationTokenSource _cts;

        private RectTransform _rect;
        private TextMeshProUGUI _textComponent;
        private Color _defaultColor;

        public RectTransform Rect => _rect;
        public TextMeshProUGUI Text => _textComponent;
        public Color DefaultColor => _defaultColor;

        private void OnEnable()
        {
            _rect = GetComponent<RectTransform>();
            _textComponent = GetComponent<TextMeshProUGUI>();
            _defaultColor = _textComponent.color;
            
            RestartAnimation();
            UpdateText("");
        }

        public void ClearText()
        {
            UpdateText(string.Empty);
        }

        /// <summary>
        /// 텍스트를 설정하고 Animation을 초기화합니다.
        /// </summary>
        public void UpdateText(string text)
        {
            if (text == string.Empty)
            {
                _cts?.Cancel();
                _textComponent.text = text;
                return;
            }
            _textComponent.text = text;
            RestartAnimation();
        }

        /// <summary>
        /// AnchoredPositin을 설정합니다.
        /// </summary>
        public void SetPosition(Vector3 position)
        {
            _rect.anchoredPosition = position;
        }

        /// <summary>
        /// DefaultColor의 알파값만을 조정합니다.
        /// </summary>
        public void SetAlpha(float alpha)
        {
            var c = _defaultColor;
            alpha = Mathf.Clamp01(alpha);
            _textComponent.faceColor = new Color(c.r, c.g, c.b, alpha);
        }


        private void RestartAnimation()
        {
            _cts?.Cancel();
            _cts = new CancellationTokenSource();
            PlayShakeAnimationAsync(_cts.Token).Forget();
        }

        private async UniTask PlayShakeAnimationAsync(CancellationToken token)
        {
            _textComponent.ForceMeshUpdate();
            var textInfo = _textComponent.textInfo;
            var cachedMeshInfo = textInfo.CopyMeshInfoVertexData(); // 원본

            while (!token.IsCancellationRequested)
            {
                if (_textComponent.havePropertiesChanged)
                {
                    _textComponent.ForceMeshUpdate();
                    textInfo = _textComponent.textInfo;
                    cachedMeshInfo = textInfo.CopyMeshInfoVertexData();
                }

                var charCount = textInfo.characterCount;

                for (int i = 0; i < charCount; i++)
                {
                    var charInfo = textInfo.characterInfo[i];
                    if (!charInfo.isVisible) continue;

                    // 원본으로 리셋
                    var materialIndex = charInfo.materialReferenceIndex;
                    var vertexIndex = charInfo.vertexIndex;

                    var sourceVertices = cachedMeshInfo[materialIndex].vertices;
                    var destinationVertices = textInfo.meshInfo[materialIndex].vertices;

                    for (int j = 0; j < 4; j++)
                        destinationVertices[vertexIndex + j] = sourceVertices[vertexIndex + j];

                    // 난수 생성
                    // 문자 중앙 X좌표(로컬)를 구해 연속적인 공간 입력으로 사용
                    var v0 = sourceVertices[vertexIndex + 0];
                    var v2 = sourceVertices[vertexIndex + 2];
                    var centerX = 0.5f * (v0.x + v2.x);
                    var jitter = MakeNoise(centerX, i);

                    // 각 정점에 동일한 오프셋 적용
                    for (int k = 0; k < 4; k++)
                        destinationVertices[vertexIndex + k] += jitter;
                }

                for (int m = 0; m < textInfo.meshInfo.Length; m++)
                {
                    var meshInfo = textInfo.meshInfo[m];
                    meshInfo.mesh.vertices = meshInfo.vertices;
                    _textComponent.UpdateGeometry(meshInfo.mesh, m);
                }

                await UniTask.DelayFrame(1, cancellationToken: token);
            }
        }

        private Vector3 MakeNoise(float centerX, int index)
        {
            var t = Time.time * shakeFrequency;
            float x = 0f, y = 0f;

            if (isWave)
            {
                float wave = Mathf.PerlinNoise(centerX * waveLength + t * waveSpeed, 0f) * 2f - 1f;
                if (shakeX) x = wave * 0.25f;
                if (shakeY) y = wave;
            }
            else
            {
                if (shakeX) x = Mathf.PerlinNoise(index * .173f, t) * 2f - 1f;
                if (shakeY) y = Mathf.PerlinNoise(index * .291f, t + 37.1f) * 2f - 1f;
            }

            return new Vector3(x, y) * shakePower;
        }

        void OnDisable()
        {
            _cts?.Cancel();
        }
    }
}
