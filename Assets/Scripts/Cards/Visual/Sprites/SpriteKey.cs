using Cardevil.Utils;
using UnityEngine;

namespace Cardevil.Cards.Visual.Sprites
{
    /// <summary>
    /// 스프라이트 키 래퍼.
    /// 문자열 키로 스프라이트 조회 기능을 제공함.
    /// </summary>
    /// <remarks>
    /// 현재는 카드 스프라이트에만 사용하고 있음.
    /// </remarks>
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