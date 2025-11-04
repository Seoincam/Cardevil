using UnityEngine;

namespace Cardevil.Utils
{
    public static class ShaderHashes
    {
        public static readonly int SHADER_COLOR =  Shader.PropertyToID("_Color");
        
    }
    
    public static class AnimatorHashes
    {
        public static readonly int IsRunning = Animator.StringToHash("IsRunning");
        public static readonly int LeftRight = Animator.StringToHash("LeftRight");
        public static readonly int UpDown = Animator.StringToHash("UpDown");
        public static readonly int Attack = Animator.StringToHash("Attack");
        public static readonly int Hit = Animator.StringToHash("Attacked");
    }
}