using Cardevil.Cards.Data;
using Cardevil.Utils;
using Cardevil.Utils.Directions;
using UnityEngine;
using UnityEngine.U2D;

namespace Cardevil.Cards.Visual.Sprites
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
        public static SpriteKey GetNumber(CardColor color, int number)
            => new($"{PrefixSingleNumber}_{color}_{number}");

        public static SpriteKey GetSmallNumber(CardColor color, int number)
            => new($"{PrefixNumber}_{color}_Small_{number}");
    
        public static SpriteKey GetQuestionMark(CardColor color)
            => new($"{PrefixNumber}_{color}_Q");

        public static SpriteKey GetStar(CardColor color)
            => new($"{PrefixSingleNumber}_{color}_Star");
    
        public static SpriteKey GetSmallStar(CardColor color)
            => new($"{PrefixNumber}_{color}_Small_Star");
    
        public static SpriteKey GetInnerFrame(CardColor color)
            => new($"{PrefixFrame}_{color}");
    
        public static SpriteKey GetEnhancementFrame(CardColor color, int level)
            => new($"{PrefixEnhancement}_{color}_{level}");
    
        // Move Card Sprites
        public static SpriteKey GetInnerFrame(Direction dir)
            => new($"{PrefixDirectionFrame}_{dir}");
    
        public static SpriteKey GetInnerFrame(DirectionFlag flag)
            => new($"{PrefixDirectionFrame}_{flag.ToCustomString()}");
    
        public static SpriteKey GetArrow(Direction dir)
            => new($"{PrefixDirectionIcon}_{dir}");
    
        public static SpriteKey GetArrow(DirectionFlag flag)
            => new($"{PrefixDirectionIcon}_{flag.ToCustomString()}");
        
        public static Sprite GetSprite(string key)
        {
            if (!TryInitialize())
                return null;
            
            return _atlas.GetSprite(key);
        }
    }
}