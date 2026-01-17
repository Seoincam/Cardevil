using Cardevil.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace Cardevil.Cards.Visual.StateMachine
{
    public readonly struct CardVisualSpriteSet
    {
        public readonly Sprite innerFrame;
        public readonly List<Sprite> sprites;
        public readonly Sprite small;

        public VisualPhase Phase => (VisualPhase)sprites.Count;

        public CardVisualSpriteSet(Sprite innerFrame, List<Sprite> sprites, Sprite small = null)
        {
            this.innerFrame = innerFrame;
            this.sprites = sprites;
            this.small = small;
        }
    }

    public readonly struct CardSpriteSet
    {
        public readonly SpriteKey InnerFrame;
        public readonly SpriteKey[] MainSprites;
        public readonly SpriteKey SmallNumber;
        
        public SpriteKey Primary => MainSprites[0];
        public VisualPhase Phase => (VisualPhase)MainSprites.Length;

        public bool HasSmallNumber => SmallNumber.IsValid;
        
        private CardSpriteSet(SpriteKey innerFrame, SpriteKey[] mainSprites, SpriteKey smallNumber)
        {
            InnerFrame = innerFrame;
            MainSprites = mainSprites;
            SmallNumber = smallNumber;
        }

        public static CardSpriteSet Single(SpriteKey innerFrame, SpriteKey mainSprite) =>
            new(innerFrame, new[] { mainSprite }, SpriteKey.Empty);
        
        public static CardSpriteSet SingleWithSmall(
            SpriteKey innerFrame, 
            SpriteKey mainSprite, 
            SpriteKey smallNumber) 
            => new (innerFrame, new[] { mainSprite }, smallNumber);

        public static CardSpriteSet Multiple(SpriteKey innerFrame, params SpriteKey[] mainSprites)
            => new(innerFrame, mainSprites, SpriteKey.Empty);
    }
    
    public readonly struct SpriteKey
    {
        private readonly string _key;
        
        public SpriteKey(string key)
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
        
        public static implicit operator Sprite(SpriteKey key) => key.Get();
        
        // 디버깅용 
        public override string ToString() => $"SpriteKey({_key})";
        
        public static readonly SpriteKey Empty = new(null);
    }
}