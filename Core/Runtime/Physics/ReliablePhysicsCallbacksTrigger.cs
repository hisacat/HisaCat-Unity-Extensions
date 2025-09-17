using UnityEngine;

namespace HisaCat.HUE.PhysicsExtension
{
    [DefaultExecutionOrder(int.MinValue)]
    public class ReliablePhysicsCallbacksTrigger : ReliablePhysicsCallbacks
    {
        [System.Serializable] public class ReliableTriggerEvent : UnityEngine.Events.UnityEvent<Collider> { }
        public ReliableTriggerEvent OnReliableTriggerEnter { get => this.m_OnReliableTriggerEnter; set => this.m_OnReliableTriggerEnter = value; }
        [SerializeField] private ReliableTriggerEvent m_OnReliableTriggerEnter = null;
        public ReliableTriggerEvent OnReliableTriggerStay { get => this.m_OnReliableTriggerStay; set => this.m_OnReliableTriggerStay = value; }
        [SerializeField] private ReliableTriggerEvent m_OnReliableTriggerStay = null;
        public ReliableTriggerEvent OnReliableTriggerExit { get => this.m_OnReliableTriggerExit; set => this.m_OnReliableTriggerExit = value; }
        [SerializeField] private ReliableTriggerEvent m_OnReliableTriggerExit = null;

        [System.Serializable] public class ReliableCollisionEvent : UnityEngine.Events.UnityEvent<Collider> { }
        public ReliableCollisionEvent OnReliableCollisionEnter { get => this.m_OnReliableCollisionEnter; set => this.m_OnReliableCollisionEnter = value; }
        [SerializeField] private ReliableCollisionEvent m_OnReliableCollisionEnter = null;
        public ReliableCollisionEvent OnReliableCollisionStay { get => this.m_OnReliableCollisionStay; set => this.m_OnReliableCollisionStay = value; }
        [SerializeField] private ReliableCollisionEvent m_OnReliableCollisionStay = null;
        public ReliableCollisionEvent OnReliableCollisionExit { get => this.m_OnReliableCollisionExit; set => this.m_OnReliableCollisionExit = value; }
        [SerializeField] private ReliableCollisionEvent m_OnReliableCollisionExit = null;

        protected override void OnReliableTriggerEnterCallback(Collider other)
            => this.OnReliableTriggerEnter.Invoke(other);
        protected override void OnReliableTriggerStayCallback(Collider other)
            => this.OnReliableTriggerStay.Invoke(other);
        protected override void OnReliableTriggerExitCallback(Collider other)
            => this.OnReliableTriggerExit.Invoke(other);

        protected override void OnReliableCollisionEnterCallback(Collider other)
            => this.OnReliableCollisionEnter.Invoke(other);
        protected override void OnReliableCollisionStayCallback(Collider other)
            => this.OnReliableCollisionStay.Invoke(other);
        protected override void OnReliableCollisionExitCallback(Collider other)
            => this.OnReliableCollisionExit.Invoke(other);
    }
}
