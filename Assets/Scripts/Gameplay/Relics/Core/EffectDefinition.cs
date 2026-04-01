using System;

namespace Cardevil.Gameplay.Relics.Core
{
    [Serializable]
    public abstract class EffectDefinition
    {
        /// <summary>
        /// 유물 에디터 상단에 표시될 이름.
        /// </summary>
        public virtual string EditorName => GetType().Name;
        
        /// <summary>
        /// 유물 에디터에 표시될 자동 요약 설명.
        /// </summary>
        public virtual string EditorDescription => "설명이 등록되지 않은 이펙트입니다.";
        
        public abstract EffectInstance CreateRuntimeInstance(RelicInstance context);
    }
}