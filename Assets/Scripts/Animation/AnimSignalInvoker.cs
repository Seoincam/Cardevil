using UnityEngine;

namespace Cardevil.Animation
{
    public class AnimSignalInvoker : StateMachineBehaviour
    {
        public string eventName;
        [Range(0.0f,1.0f)] public float eventTimePercent;
        [HideInInspector] public bool hasInvoked = false;
        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo,
            int layerIndex)
        {
            
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo,
            int layerIndex)
        {
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo,
            int layerIndex)
        {
            if (!hasInvoked && stateInfo.normalizedTime >= eventTimePercent)
            {
                var listeners = animator.GetComponentsInChildren<IAnimSignalListener>();
                foreach (var listener in listeners)
                {
                    listener.OnSignalEvent(eventName);
                }
                hasInvoked = true;
            }
        }

        public override void OnStateMove(Animator animator, AnimatorStateInfo stateInfo,
            int layerIndex)
        {
        }

        public override void OnStateIK(Animator animator, AnimatorStateInfo stateInfo,
            int layerIndex)
        {
        }
    }
}