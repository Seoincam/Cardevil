using Cardevil.Core.Attributes;
using Cardevil.Core.Systems.Pool;
using Cardevil.Core.Utils;
using UnityEngine;

namespace Cardevil.Gameplay.Field
{
    [RequireComponent(typeof(Poolable))]
    public class TileHighlight : MonoBehaviour
    {
        [VisibleOnly(EditableIn.EditMode), SerializeField] private SpriteRenderer _spriteRenderer;
        [VisibleOnly(EditableIn.EditMode), SerializeField] private Poolable _poolable;
        [SerializeField] private FieldConfigurationSO _fieldConfigurationSo;
        [SerializeField] private Define.HighlightType _highlightType = Define.HighlightType.None;
        
        public Tile Tile { get; private set; }
        public SpriteRenderer SpriteRenderer => _spriteRenderer;
        public Poolable Poolable => _poolable;
        public Define.HighlightType HighlightType => _highlightType;

        private void Awake()
        {
            if (_spriteRenderer == null)
            {
                _spriteRenderer = GetComponent<SpriteRenderer>();
            }
            if (_poolable == null)
            {
                _poolable = GetComponent<Poolable>();
            }

            _poolable.OnGet += () =>
            {
                _highlightType = Define.HighlightType.None;
            };
        }
        
        public void Initialize(Tile tile, FieldConfigurationSO fieldConfigurationSo)
        {
            Tile = tile;
            _fieldConfigurationSo = fieldConfigurationSo;
        }
        
        public void SetHighlightColor(Define.HighlightType type, Color color)
        {
            _highlightType = type;
            if (_spriteRenderer != null)
            {
                _spriteRenderer.color = color;
            }
        }
    }
}