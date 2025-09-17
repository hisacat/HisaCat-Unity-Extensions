using UnityEngine;

namespace HisaCat.HUE.PhysicsExtension
{
    [DefaultExecutionOrder(int.MinValue)]
    public class ReliablePhysics2DCallbacksTrigger : ReliablePhysics2DCallbacks
    {
        [System.Serializable] public class ReliableTriggerEvent : UnityEngine.Events.UnityEvent<Collider2D> { }
        public ReliableTriggerEvent OnReliableTrigger2DEnter { get => this.m_OnReliableTriggerEnter; set => this.m_OnReliableTriggerEnter = value; }
        [SerializeField] private ReliableTriggerEvent m_OnReliableTriggerEnter = null;
        public ReliableTriggerEvent OnReliableTrigger2DStay { get => this.m_OnReliableTriggerStay; set => this.m_OnReliableTriggerStay = value; }
        [SerializeField] private ReliableTriggerEvent m_OnReliableTriggerStay = null;
        public ReliableTriggerEvent OnReliableTrigger2DExit { get => this.m_OnReliableTriggerExit; set => this.m_OnReliableTriggerExit = value; }
        [SerializeField] private ReliableTriggerEvent m_OnReliableTriggerExit = null;

        [System.Serializable] public class ReliableCollisionEvent : UnityEngine.Events.UnityEvent<Collider2D> { }
        public ReliableCollisionEvent OnReliableCollision2DEnter { get => this.m_OnReliableCollisionEnter; set => this.m_OnReliableCollisionEnter = value; }
        [SerializeField] private ReliableCollisionEvent m_OnReliableCollisionEnter = null;
        public ReliableCollisionEvent OnReliableCollision2DStay { get => this.m_OnReliableCollisionStay; set => this.m_OnReliableCollisionStay = value; }
        [SerializeField] private ReliableCollisionEvent m_OnReliableCollisionStay = null;
        public ReliableCollisionEvent OnReliableCollision2DExit { get => this.m_OnReliableCollisionExit; set => this.m_OnReliableCollisionExit = value; }
        [SerializeField] private ReliableCollisionEvent m_OnReliableCollisionExit = null;

        protected override void OnReliableTrigger2DEnterCallback(Collider2D other)
            => this.OnReliableTrigger2DEnter.Invoke(other);
        protected override void OnReliableTrigger2DStayCallback(Collider2D other)
            => this.OnReliableTrigger2DStay.Invoke(other);
        protected override void OnReliableTrigger2DExitCallback(Collider2D other)
            => this.OnReliableTrigger2DExit.Invoke(other);

        protected override void OnReliableCollision2DEnterCallback(Collider2D other)
            => this.OnReliableCollision2DEnter.Invoke(other);
        protected override void OnReliableCollision2DStayCallback(Collider2D other)
            => this.OnReliableCollision2DStay.Invoke(other);
        protected override void OnReliableCollision2DExitCallback(Collider2D other)
            => this.OnReliableCollision2DExit.Invoke(other);
    }
}
