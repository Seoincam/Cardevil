using Cardevil.Animation;
using Cardevil.DebugConsole;
using Cardevil.Utils;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace Cardevil.Ingame.Entities
{
    [RequireComponent(typeof(SpriteRenderer), typeof(Animator))]
    public class PlayerVisual : MonoBehaviour, IAnimSignalListener
    {
        [SerializeField] private Animator _animator;
        [SerializeField] private SpriteRenderer _mainSpriteRenderer;

        public bool IsRunning
        {
            get => _animator.GetBool(Utils.AnimatorHashes.IsRunning);
            set => _animator.SetBool(Utils.AnimatorHashes.IsRunning, value);
        }
        
        public Vector2 MoveDirection
        {
            get => new Vector2(_animator.GetFloat(Utils.AnimatorHashes.LeftRight), _animator.GetFloat(Utils.AnimatorHashes.UpDown));
            set
            {
                _animator.SetFloat(Utils.AnimatorHashes.LeftRight, value.x);
                _animator.SetFloat(Utils.AnimatorHashes.UpDown, -value.y);
            }
        }
        
        public void PlayAttackAnimation()
        {
            _animator.SetTrigger(Utils.AnimatorHashes.Attack);
        }
        
        public void PlayHitAnimation()
        {
            _animator.SetTrigger(Utils.AnimatorHashes.Hit);
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


        [ConsoleCommand("PlayerVisual",
            "손님 플레이어 비주얼 테스트",
            "PlayerVisual <type>(run/idle/attack/hit/rotate)",
            new[] { "run", "idle", "attack", "hit", "rotate" })]
        private static void TestPlayerVisual(string type)
        {
            switch (type)
            {
                case "run":
                    Managers.Game.Player.PlayerVisual.IsRunning = true;
                    break;
                case "idle":
                    Managers.Game.Player.PlayerVisual.IsRunning = false;
                    break;
                case "attack":
                    Managers.Game.Player.PlayerVisual.PlayAttackAnimation();
                    break;
                case "hit":
                    Managers.Game.Player.PlayerVisual.PlayHitAnimation();
                    break;
                case "rotate":
                    var pv = Managers.Game.Player.PlayerVisual;
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
        
    }
}