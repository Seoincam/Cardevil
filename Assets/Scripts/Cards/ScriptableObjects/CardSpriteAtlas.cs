using Cardevil.Attributes;
using Cardevil.Cards.Data;
using Cardevil.DataStructure.Serializables;
using Cardevil.Utils;
using Cardevil.Utils.Directions;
using UnityEngine;
using UnityEngine.U2D;

namespace Cardevil.Cards.ScriptableObjects
{
    public class CardSpriteAtlas : ScriptableObject
    {
        [SerializeField] private SpriteAtlas atlas;
        [SerializeField, VisibleOnly] private SerializableDictionary<string, Sprite> cache;

        private void OnEnable()
        {
            Warmup();
        }
        
        private void Warmup()
        {
            if (cache != null) return;
            
            cache = new();
            var arr = new Sprite[atlas.spriteCount];
            atlas.GetSprites(arr);
            foreach (var sprite in arr)
                cache[sprite.name] = sprite;
        }

        // Attack Card Sprites
        public Sprite GetNumber(CardColor color, int number)
        {
            var s = $"Card_Single_Number_{color}_{number}";
            return GetSprite(s);
        }
        public Sprite GetQuestionMark(CardColor color)
        {
            var s = $"Card_Single_Number_{color}_Q";
            return GetSprite(s);
        }
        public Sprite GetStar(CardColor color)
        {
            var s = $"Card_Single_Number_{color}_Star";
            return GetSprite(s);
        }
        public Sprite GetInnerFrame(CardColor color)
        {
            var s = $"Card_Frame_{color}";
            return GetSprite(s);
        }
        public Sprite GetEnhancementFrame(CardColor color, int level)
        {
            var s = $"Card_Enhancement_{color}_{level}";
            return GetSprite(s);
        }
        
        // Move Card Sprites
        public Sprite GetInnerFrame(DirectionFlag flag)
        {
            var s = $"Card_Direction_Frame_{flag}";
            return GetSprite(s);
        }
        public Sprite GetArrow(DirectionFlag flag)
        {
            var s = $"Card_Direction_Icon_{flag}";
            return GetSprite(s);
        }
        
        private Sprite GetSprite(string spriteName)
        {
            if (!cache.TryGetValue(spriteName, out var sprite))
            {
                LogEx.LogWarning("No sprite found for: " + spriteName);
                return null;
            }
            
            return sprite;
        }
    }
}