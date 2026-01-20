using UnityEngine;

namespace Cardevil.Utils
{
    public static class Hashes
    {
        public static readonly int SHADER_COLOR =  Shader.PropertyToID("_BaseColor");
        
    }
    
    public static class AnimatorHashes
    {
        // Player
        public static readonly int IsRunning = Animator.StringToHash("IsRunning");
        public static readonly int LeftRight = Animator.StringToHash("LeftRight");
        public static readonly int UpDown = Animator.StringToHash("UpDown");
        public static readonly int Attack = Animator.StringToHash("Attack");
        public static readonly int Hit = Animator.StringToHash("Attacked");
        public static readonly int IsFalling = Animator.StringToHash("IsFalling");
        
        // RockPile
        public static readonly int Break = Animator.StringToHash("Break");
    }
}