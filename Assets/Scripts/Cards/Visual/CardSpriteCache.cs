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
        private static SpriteAtlas _atlas; private static bool _isInitialized;

        private const string AtlasPath = "Arts/CardAtlas";
        private const string CloneSuffix = "(Clone)";

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
            var s = $"Card_Direction_Frame_{flag.ToCustomString()}";
            return GetSprite(s);
        }
        public static Sprite GetArrow(DirectionFlag flag)
        {
            var s = $"Card_Direction_Icon_{flag.ToCustomString()}";
            return GetSprite(s);
        }
        
        private static Sprite GetSprite(string key)
        {
            if (!TryInitialize())
                return null;
            
            return _atlas.GetSprite(key);
        }

        private static string NormalizeSpriteName(string rawName)
        {
            if (rawName.EndsWith(CloneSuffix))
                rawName = rawName[..^CloneSuffix.Length];
            return rawName.Trim('"', ' ');
        }
    }
}