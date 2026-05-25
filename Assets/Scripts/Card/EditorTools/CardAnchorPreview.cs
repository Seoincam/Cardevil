using UnityEngine;
using Cardevil.Card.Common.Core;
using Cardevil.Card.Common.Core.Upgrade;
using Cardevil.Card.Common.Visual;
using Cardevil.Card.Visual.Controller;
using Cardevil.Core.Utils;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Cardevil.Card.EditorTools
{
    [ExecuteAlways]
    public class CardAnchorPreview : MonoBehaviour
    {
        public enum PreviewMode
        {
            Random,
            Custom
        }

        [Header("Prefab")]
        public InteractionCard cardPrefab;

        private CardVisualInput _cachedRandomInput;
        private bool _hasCachedRandomInput;

        [Header("Settings")]
        public bool showPreview = true;
        public PreviewMode mode = PreviewMode.Random;
        public float cardScale = 1f;

        [Header("Custom Settings")]
        public CardType customType = CardType.Attack;
        public UpgradePath customUpgrade = UpgradePath.None;
        
        [Range(1, 3)]
        public int customElementCount = 1;

        public CardColor baseColor = CardColor.Red;
        
        public CardColor customColor1 = CardColor.Red;
        public CardColor customColor2 = CardColor.Red;
        public CardColor customColor3 = CardColor.Blue;

        public int customNumber1 = 2;
        public int customNumber2 = 3;
        public int customNumber3 = 4;

        public Direction customDir1 = Direction.Up;
        public Direction customDir2 = Direction.Down;
        public Direction customDir3 = Direction.Left;

        private InteractionCard _previewInstance;

        private void OnEnable()
        {
            if (_previewInstance == null && !Application.isPlaying)
            {
#if UNITY_EDITOR
                EditorApplication.delayCall -= DelayGeneratePreview;
                EditorApplication.delayCall += DelayGeneratePreview;
#endif
            }
        }

        private void DelayGeneratePreview()
        {
            if (this != null && enabled && showPreview && cardPrefab != null)
            {
                GeneratePreview(false);
            }
        }

        private void OnDisable()
        {
            ClearPreview();
        }

        private void OnDestroy()
        {
            ClearPreview();
        }

        public void ClearPreview()
        {
            if (_previewInstance != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(_previewInstance.gameObject);
                }
                else
                {
                    DestroyImmediate(_previewInstance.gameObject);
                }
                _previewInstance = null;
            }
        }

        public void GeneratePreview(bool forceRandomize = false)
        {
            if (cardPrefab == null)
            {
                Debug.LogWarning("CardAnchorPreview: Card Prefab is missing.");
                return;
            }

            ClearPreview();

            _previewInstance = Instantiate(cardPrefab, transform);
            _previewInstance.gameObject.hideFlags = HideFlags.DontSave;
            
            _previewInstance.transform.localPosition = Vector3.zero;
            _previewInstance.transform.localRotation = Quaternion.identity;
            _previewInstance.transform.localScale = Vector3.one * cardScale;

            CardVisualInput input;

            if (mode == PreviewMode.Random)
            {
                if (forceRandomize || !_hasCachedRandomInput)
                {
                    _cachedRandomInput = GenerateRandomInput();
                    _hasCachedRandomInput = true;
                }
                input = _cachedRandomInput;
            }
            else
            {
                input = GenerateCustomInput();
            }

            _previewInstance.Initialize(input, false, 5); 
            _previewInstance.VisualController.SetSortingOrder(10);
            _previewInstance.VisualController.Fade(1f, true);
        }

        private CardVisualInput GenerateRandomInput()
        {
            CardType type = (Random.value > 0.5f) ? CardType.Attack : CardType.Move;

            if (type == CardType.Attack)
            {
                bool multiColor = Random.value > 0.5f;
                int elementCount = Random.Range(1, 4);
                
                if (multiColor)
                {
                    CardColor[] allColors = { CardColor.Black, CardColor.Red, CardColor.Blue, CardColor.Green };
                    CardColor?[] colors = new CardColor?[elementCount];
                    for (int i = 0; i < elementCount; i++) colors[i] = allColors[Random.Range(0, allColors.Length)];
                    
                    return CardVisualInput.Attack(colors[0] ?? CardColor.Red, colors, Random.Range(2, 11));
                }
                else
                {
                    CardColor color = (CardColor)Random.Range(1, 5); // 1 = Red, 2 = Green, 3 = Blue, 4 = Black
                    int?[] nums = new int?[elementCount];
                    for (int i = 0; i < elementCount; i++) nums[i] = Random.Range(2, 11);
                    return CardVisualInput.Attack(color, nums);
                }
            }
            else
            {
                int dirCount = Random.Range(1, 4);
                if (dirCount == 1)
                {
                    Direction[] allDirs = { Direction.Up, Direction.Down, Direction.Left, Direction.Right };
                    return CardVisualInput.Move(allDirs[Random.Range(0, 4)]);
                }
                else if (dirCount == 2)
                {
                    bool isUpDown = Random.value > 0.5f;
                    DirectionFlag flag = isUpDown ? DirectionFlag.UpDown : DirectionFlag.LeftRight;
                    Direction[] dirs = isUpDown ? new[] { Direction.Up, Direction.Down } : new[] { Direction.Left, Direction.Right };
                    return CardVisualInput.Move(flag, dirs);
                }
                else
                {
                    Direction[] dirs = { Direction.Up, Direction.Down, Direction.Left, Direction.Right };
                    return CardVisualInput.Move(DirectionFlag.All, dirs);
                }
            }
        }

        private CardVisualInput GenerateCustomInput()
        {
            if (customType == CardType.Attack)
            {
                if (customUpgrade == UpgradePath.MultiColor)
                {
                    CardColor?[] colors = new CardColor?[customElementCount];
                    if (customElementCount >= 1) colors[0] = customColor1;
                    if (customElementCount >= 2) colors[1] = customColor2;
                    if (customElementCount >= 3) colors[2] = customColor3;
                    return CardVisualInput.Attack(baseColor, colors, customNumber1);
                }
                else // None or MultiNumber
                {
                    int?[] nums = new int?[customElementCount];
                    if (customElementCount >= 1) nums[0] = customNumber1;
                    if (customElementCount >= 2) nums[1] = customNumber2;
                    if (customElementCount >= 3) nums[2] = customNumber3;
                    return CardVisualInput.Attack(baseColor, nums);
                }
            }
            else
            {
                int count = customUpgrade == UpgradePath.MultiDirection ? customElementCount : 1;
                if (count <= 1)
                {
                    return CardVisualInput.Move(customDir1);
                }
                else if (count == 2)
                {
                    DirectionFlag flag = customDir1.ToDirectionFlag() | customDir2.ToDirectionFlag();
                    if (flag != DirectionFlag.UpDown && flag != DirectionFlag.LeftRight)
                    {
                        // Invalid combination fallback
                        flag = DirectionFlag.UpDown; 
                        customDir1 = Direction.Up;
                        customDir2 = Direction.Down;
                    }
                    return CardVisualInput.Move(flag, customDir1, customDir2);
                }
                else
                {
                    return CardVisualInput.Move(DirectionFlag.All, Direction.Up, Direction.Down, Direction.Left, Direction.Right);
                }
            }
        }
    }
}
