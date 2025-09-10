using UnityEngine;

namespace HisaCat.StateMachineBehaviours
{
    public class CancelTrigger : StateMachineBehaviour
    {
        [SerializeField] private string m_TriggerName = null;
        [SerializeField] private bool m_CancelOnEnter = false;
        [SerializeField] private bool m_CancelOnExit = false;
        //[SerializeField] private bool m_CancelOnUpdate = false;

        private int triggerHash = 0;
        private void Awake()
        {
            this.triggerHash = Animator.StringToHash(this.m_TriggerName);
        }

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (this.m_CancelOnEnter) animator.ResetTrigger(this.triggerHash);
        }
        //public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        //{
        //    if (this.m_CancelOnUpdate) animator.ResetTrigger(this.triggerHash);
        //}
        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (this.m_CancelOnExit) animator.ResetTrigger(this.triggerHash);
        }
    }
}
