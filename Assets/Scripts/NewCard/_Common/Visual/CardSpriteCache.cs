using Cardevil.NewCard.Common.Core;
using Cardevil.Utils;
using Cardevil.Utils.Directions;
using UnityEngine;
using UnityEngine.U2D;

namespace Cardevil.NewCard.Common.Visual
{
    public static class CardSpriteCache
    {
        private static SpriteAtlas _atlas; 
        private static bool _isInitialized;
        
        private const string AtlasPath = "Arts/CardAtlas";
        private const string PrefixSingleNumber = "Card_Single_Number";
        private const string PrefixNumber = "Card_Number";
        private const string PrefixFrame = "Card_Frame";
        private const string PrefixEnhancement = "Card_Enhancement";
        private const string PrefixDirectionFrame = "Card_Direction_Frame";
        private const string PrefixDirectionIcon = "Card_Direction_Icon";

        private static bool TryInitialize()
        {
            if (_isInitialized)
                return true;
            
            _atlas = Resources.Load<SpriteAtlas>(AtlasPath);
            if (!_atlas)
            {
                LogEx.LogError("Atlas not found: Resources/" + AtlasPath);
                return false;
            }
            
            _isInitialized = true;
            return true;
        }
    
        // Attack Card Sprites
        public static SpriteReference GetNumber(CardColor color, int number)
            => new($"{PrefixSingleNumber}_{color}_{number}");

        public static SpriteReference GetSmallNumber(CardColor color, int number)
            => new($"{PrefixNumber}_{color}_Small_{number}");
    
        public static SpriteReference GetQuestionMark(CardColor color)
            => new($"{PrefixNumber}_{color}_Q");

        public static SpriteReference GetStar(CardColor color)
            => new($"{PrefixSingleNumber}_{color}_Star");
    
        public static SpriteReference GetSmallStar(CardColor color)
            => new($"{PrefixNumber}_{color}_Small_Star");
    
        public static SpriteReference GetInnerFrame(CardColor color)
            => new($"{PrefixFrame}_{color}");
    
        public static SpriteReference GetEnhancementFrame(CardColor color, int level)
            => new($"{PrefixEnhancement}_{color}_{level}");
    
        // Move Card Sprites
        public static SpriteReference GetInnerFrame(Direction dir)
            => new($"{PrefixDirectionFrame}_{dir}");
    
        public static SpriteReference GetInnerFrame(DirectionFlag flag)
            => new($"{PrefixDirectionFrame}_{flag.ToCustomString()}");
    
        public static SpriteReference GetArrow(Direction dir)
            => new($"{PrefixDirectionIcon}_{dir}");
    
        public static SpriteReference GetArrow(DirectionFlag flag)
            => new($"{PrefixDirectionIcon}_{flag.ToCustomString()}");
        
        public static Sprite GetSprite(string key)
        {
            if (!TryInitialize())
                return null;
            
            return _atlas.GetSprite(key);
        }
    }
    
    /// <summary>
    /// 스프라이트 키 래퍼.
    /// 문자열 키로 스프라이트 조회 기능을 제공함.
    /// </summary>
    public readonly struct SpriteReference
    {
        private readonly string _key;
        
        public SpriteReference(string key)
        {
            _key = key;
        }

        public bool IsValid => !string.IsNullOrEmpty(_key);
        
        /// <summary>
        /// 스프라이트를 가져옴. (null 가능)
        /// </summary>
        public Sprite Get() => CardSpriteCache.GetSprite(_key);

        /// <summary>
        /// 스프라이트 가져오기 (null이면 에러 로그)
        /// </summary>
        public Sprite GetOrLogError()
        {
            var sprite = Get();
            if (!sprite && IsValid)
                LogEx.LogError($"Sprite not found for key: {_key}");
            return sprite;
        }

        public bool TryGet(out Sprite sprite)
        {
            sprite = Get();
            return sprite;
        }
        
        public static implicit operator Sprite(SpriteReference reference) => reference.Get();
        
        // 디버깅용 
        public override string ToString() => $"SpriteKey({_key})";
        
        public static readonly SpriteReference Empty = new(null);
    }
}