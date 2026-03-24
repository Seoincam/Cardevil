using Cardevil.Core.Bootstrap;
using Cardevil.Core.Systems.Sounds;
using Cardevil.Core.Utils;
using Cardevil.Gameplay.Animation;
using UnityEngine;
using UnityEngine.Events;

namespace Cardevil.Gameplay.Player
{
    [RequireComponent(typeof(SpriteRenderer), typeof(Animator))]
    public class PlayerVisual : MonoBehaviour, IAnimSignalListener
    {
        [SerializeField] private Animator _animator;
        [SerializeField] private SpriteRenderer _mainSpriteRenderer;
        [SerializeField] private SpriteRenderer _shadowSpriteRenderer;

        public bool IsRunning
        {
            get => _animator.GetBool(AnimatorHashes.IsRunning);
            set => _animator.SetBool(AnimatorHashes.IsRunning, value);
        }
        
        public bool IsFalling
        {
            get => _animator.GetBool(AnimatorHashes.IsFalling);
            set => _animator.SetBool(AnimatorHashes.IsFalling, value);
        }
        
        public Vector2 MoveDirection
        {
            get => new Vector2(_animator.GetFloat(AnimatorHashes.LeftRight), _animator.GetFloat(AnimatorHashes.UpDown));
            set
            {
                _animator.SetFloat(AnimatorHashes.LeftRight, value.x);
                _animator.SetFloat(AnimatorHashes.UpDown, -value.y);
            }
        }
        
        public float FadeAlpha
        {
            get => _mainSpriteRenderer.color.a;
            set => SetFade(value);
        }
        
        public float ShadowFadeAlpha
        {
            get => _shadowSpriteRenderer.color.a;
            set
            {
                Color color = _shadowSpriteRenderer.color;
                color.a = value;
                _shadowSpriteRenderer.color = color;
            }
        }
        
        public float ShadowScale
        {
            get => _shadowSpriteRenderer.transform.localScale.x;
            set => _shadowSpriteRenderer.transform.localScale = new Vector3(value, value, 1f);
        }
        
        public void PlayAttackAnimation()
        {
            _animator.SetTrigger(AnimatorHashes.Attack);
            
        }
        
        public void PlayHitAnimation()
        {
            _animator.SetTrigger(AnimatorHashes.Hit);
            
        }
        
        public UnityEvent<string> animEventReceived = new UnityEvent<string>();
        
        private void Reset()
        {
            _animator = GetComponent<Animator>();
            _mainSpriteRenderer = GetComponent<SpriteRenderer>();
        }
        
        public void OnSignalEvent(string eventName)
        {
            LogEx.Log($"Anim Event Received: {eventName}");
            animEventReceived?.Invoke(eventName);
        }

        /*
        [ConsoleCommand("PlayerVisual",
            "손님 플레이어 비주얼 테스트",
            "PlayerVisual <type>(run/idle/attack/hit/rotate)",
            new[] { "run", "idle", "attack", "hit", "rotate" })]
        private static void TestPlayerVisual(string type)
        {
            switch (type)
            {
                case "run":
                    Bootstrapper.Instance.Game.Player.PlayerVisual.IsRunning = true;
                    break;
                case "idle":
                    Bootstrapper.Instance.Game.Player.PlayerVisual.IsRunning = false;
                    break;
                case "attack":
                    Bootstrapper.Instance.Game.Player.PlayerVisual.PlayAttackAnimation();
                    break;
                case "hit":
                    Bootstrapper.Instance.Game.Player.PlayerVisual.PlayHitAnimation();
                    break;
                case "rotate":
                    var pv = Bootstrapper.Instance.Game.Player.PlayerVisual;
                    Vector2 currentDir = pv.MoveDirection;
                    Vector2 newDir = new Vector2(-currentDir.y, currentDir.x); // 90도 회전
                    pv.MoveDirection = newDir;
                    LogEx.Log($"Rotated MoveDirection to: {newDir}");
                    break;
                default:
                    LogEx.LogError("Unknown PlayerVisual test type: " + type);
                    break;
            }
        }
        */
        
        public void SetFade(float alpha)
        {
            Color color = _mainSpriteRenderer.color;
            color.a = alpha;
            _mainSpriteRenderer.color = color;
        }
        
    }
}