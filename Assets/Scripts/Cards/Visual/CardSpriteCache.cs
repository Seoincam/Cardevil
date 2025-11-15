using Cardevil.Cards.Data;
using Cardevil.Utils;
using Cardevil.Utils.Directions;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

namespace Cardevil.Cards.Visual
{
    public static class CardSpriteCache
    {
        private static SpriteAtlas _atlas;
        private static Dictionary<string, Sprite> _cache;
        private static bool _isInitialized;

        private const string AtlasPath = "Arts/CardAtlas";
        private const string CloneSuffix = "(Clone)";

        private static bool TryInitialize()
        {
            if (_isInitialized)
                return true;
            
            _atlas = Resources.Load<SpriteAtlas>(AtlasPath);
            if (_atlas == null)
            {
                LogEx.LogError("Atlas not found: Resources/" + AtlasPath);
                return false;
            }
            
            _cache = new Dictionary<string, Sprite>();
            var arr = new Sprite[_atlas.spriteCount];
            _atlas.GetSprites(arr);
            foreach (var sprite in arr)
            {
                var name = NormalizeSpriteName(sprite.name);
                _cache[name] = sprite;
            }

            _isInitialized = true;
            return true;
        }
        
        // Attack Card Sprites
        public static Sprite GetNumber(CardColor color, int number)
        {
            var s = $"Card_Single_Number_{color}_{number}";
            return GetSprite(s);
        }
        public static Sprite GetQuestionMark(CardColor color)
        {
            var s = $"Card_Number_{color}_Q";
            return GetSprite(s);
        }
        public static Sprite GetStar(CardColor color)
        {
            var s = $"Card_Single_Number_{color}_Star";
            return GetSprite(s);
        }
        public static Sprite GetInnerFrame(CardColor color)
        {
            var s = $"Card_Frame_{color}";
            return GetSprite(s);
        }
        public static Sprite GetEnhancementFrame(CardColor color, int level)
        {
            var s = $"Card_Enhancement_{color}_{level}";
            return GetSprite(s);
        }
        
        // Move Card Sprites
        public static Sprite GetInnerFrame(DirectionFlag flag)
        {
            var s = $"Card_Direction_Frame_{flag}";
            return GetSprite(s);
        }
        public static Sprite GetArrow(DirectionFlag flag)
        {
            var s = $"Card_Direction_Icon_{flag}";
            return GetSprite(s);
        }
        
        private static Sprite GetSprite(string spriteName)
        {
            if (!TryInitialize())
                return null;
            
            if (!_cache.TryGetValue(spriteName, out var sprite))
            {
                LogEx.LogWarning("No sprite found for: " + spriteName);
                return null;
            }
            
            return sprite;
        }

        private static string NormalizeSpriteName(string rawName)
        {
            if (rawName.EndsWith(CloneSuffix))
                rawName = rawName[..^CloneSuffix.Length];
            return rawName.Trim('"', ' ');
        }
    }
}