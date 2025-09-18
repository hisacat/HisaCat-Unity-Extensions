using HisaCat.HUE.Collections;
using UnityEngine;

namespace HisaCat.HUE.PhysicsExtension
{
    [DefaultExecutionOrder(int.MinValue)]
    public class ReliablePhysics2DUnityEvents : ReliablePhysics2DCallbacks
    {
        [System.Serializable] public class ReliableTrigger2DEvent : UnityEngine.Events.UnityEvent<Collider2D> { }
        public ReliableTrigger2DEvent OnReliableTrigger2DEnter { get => this.m_OnReliableTriggerEnter; set => this.m_OnReliableTriggerEnter = value; }
        [SerializeField] private ReliableTrigger2DEvent m_OnReliableTriggerEnter = new();
        public ReliableTrigger2DEvent OnReliableTrigger2DStay { get => this.m_OnReliableTriggerStay; set => this.m_OnReliableTriggerStay = value; }
        [SerializeField] private ReliableTrigger2DEvent m_OnReliableTriggerStay = new();
        public ReliableTrigger2DEvent OnReliableTrigger2DExit { get => this.m_OnReliableTriggerExit; set => this.m_OnReliableTriggerExit = value; }
        [SerializeField] private ReliableTrigger2DEvent m_OnReliableTriggerExit = new();
        [System.Serializable] public class ReliableTriggerStayingChanged2DEvent : UnityEngine.Events.UnityEvent<IReadOnlyHashSet<Collider2D>> { }
        public ReliableTriggerStayingChanged2DEvent OnReliableTriggerStayingChanged2D { get => this.m_OnReliableTriggerStayingChanged2D; set => this.m_OnReliableTriggerStayingChanged2D = value; }
        [SerializeField] private ReliableTriggerStayingChanged2DEvent m_OnReliableTriggerStayingChanged2D = new();

        [System.Serializable] public class ReliableCollision2DEvent : UnityEngine.Events.UnityEvent<Collider2D> { }
        public ReliableCollision2DEvent OnReliableCollision2DEnter { get => this.m_OnReliableCollisionEnter; set => this.m_OnReliableCollisionEnter = value; }
        [SerializeField] private ReliableCollision2DEvent m_OnReliableCollisionEnter = new();
        public ReliableCollision2DEvent OnReliableCollision2DStay { get => this.m_OnReliableCollisionStay; set => this.m_OnReliableCollisionStay = value; }
        [SerializeField] private ReliableCollision2DEvent m_OnReliableCollisionStay = new();
        public ReliableCollision2DEvent OnReliableCollision2DExit { get => this.m_OnReliableCollisionExit; set => this.m_OnReliableCollisionExit = value; }
        [SerializeField] private ReliableCollision2DEvent m_OnReliableCollisionExit = new();
        [System.Serializable] public class ReliableCollisionStayingChanged2DEvent : UnityEngine.Events.UnityEvent<IReadOnlyHashSet<Collider2D>> { }
        public ReliableCollisionStayingChanged2DEvent OnReliableCollisionStayingChanged2D { get => this.m_OnReliableCollisionStayingChanged2D; set => this.m_OnReliableCollisionStayingChanged2D = value; }
        [SerializeField] private ReliableCollisionStayingChanged2DEvent m_OnReliableCollisionStayingChanged2D = new();

        protected override void OnReliableTrigger2DEnterCallback(Collider2D other)
            => this.OnReliableTrigger2DEnter.Invoke(other);
        protected override void OnReliableTrigger2DStayCallback(Collider2D other)
            => this.OnReliableTrigger2DStay.Invoke(other);
        protected override void OnReliableTrigger2DExitCallback(Collider2D other)
            => this.OnReliableTrigger2DExit.Invoke(other);
        protected override void OnReliableTriggerStayingChanged2DCallback(IReadOnlyHashSet<Collider2D> staying)
            => this.OnReliableTriggerStayingChanged2D.Invoke(staying);

        protected override void OnReliableCollision2DEnterCallback(Collider2D other)
            => this.OnReliableCollision2DEnter.Invoke(other);
        protected override void OnReliableCollision2DStayCallback(Collider2D other)
            => this.OnReliableCollision2DStay.Invoke(other);
        protected override void OnReliableCollision2DExitCallback(Collider2D other)
            => this.OnReliableCollision2DExit.Invoke(other);
        protected override void OnReliableCollisionStayingChanged2DCallback(IReadOnlyHashSet<Collider2D> staying)
            => this.OnReliableCollisionStayingChanged2D.Invoke(staying);
    }
}
