using HisaCat.HUE.Collections;
using UnityEngine;

namespace HisaCat.HUE.PhysicsExtension
{
    [DefaultExecutionOrder(int.MinValue)]
    public class ReliablePhysicsUnityEvents : ReliablePhysicsCallbacks
    {
        [System.Serializable] public class ReliableTriggerEvent : UnityEngine.Events.UnityEvent<Collider> { }
        public ReliableTriggerEvent OnReliableTriggerEnter { get => this.m_OnReliableTriggerEnter; set => this.m_OnReliableTriggerEnter = value; }
        [SerializeField] private ReliableTriggerEvent m_OnReliableTriggerEnter = new();
        public ReliableTriggerEvent OnReliableTriggerStay { get => this.m_OnReliableTriggerStay; set => this.m_OnReliableTriggerStay = value; }
        [SerializeField] private ReliableTriggerEvent m_OnReliableTriggerStay = new();
        public ReliableTriggerEvent OnReliableTriggerExit { get => this.m_OnReliableTriggerExit; set => this.m_OnReliableTriggerExit = value; }
        [SerializeField] private ReliableTriggerEvent m_OnReliableTriggerExit = new();
        [System.Serializable] public class ReliableTriggerStayingChangedEvent : UnityEngine.Events.UnityEvent<IReadOnlyHashSet<Collider>> { }
        public ReliableTriggerStayingChangedEvent OnReliableTriggerStayingChanged { get => this.m_OnReliableTriggerStayingChanged; set => this.m_OnReliableTriggerStayingChanged = value; }
        [SerializeField] private ReliableTriggerStayingChangedEvent m_OnReliableTriggerStayingChanged = new();

        [System.Serializable] public class ReliableCollisionEvent : UnityEngine.Events.UnityEvent<Collider> { }
        public ReliableCollisionEvent OnReliableCollisionEnter { get => this.m_OnReliableCollisionEnter; set => this.m_OnReliableCollisionEnter = value; }
        [SerializeField] private ReliableCollisionEvent m_OnReliableCollisionEnter = new();
        public ReliableCollisionEvent OnReliableCollisionStay { get => this.m_OnReliableCollisionStay; set => this.m_OnReliableCollisionStay = value; }
        [SerializeField] private ReliableCollisionEvent m_OnReliableCollisionStay = new();
        public ReliableCollisionEvent OnReliableCollisionExit { get => this.m_OnReliableCollisionExit; set => this.m_OnReliableCollisionExit = value; }
        [SerializeField] private ReliableCollisionEvent m_OnReliableCollisionExit = new();
        [System.Serializable] public class ReliableCollisionStayingChangedEvent : UnityEngine.Events.UnityEvent<IReadOnlyHashSet<Collider>> { }
        public ReliableCollisionStayingChangedEvent OnReliableCollisionStayingChanged { get => this.m_OnReliableCollisionStayingChanged; set => this.m_OnReliableCollisionStayingChanged = value; }
        [SerializeField] private ReliableCollisionStayingChangedEvent m_OnReliableCollisionStayingChanged = new();

        protected override void OnReliableTriggerEnterCallback(Collider other)
            => this.OnReliableTriggerEnter.Invoke(other);
        protected override void OnReliableTriggerStayCallback(Collider other)
            => this.OnReliableTriggerStay.Invoke(other);
        protected override void OnReliableTriggerExitCallback(Collider other)
            => this.OnReliableTriggerExit.Invoke(other);
        protected override void OnReliableTriggerStayingChangedCallback(IReadOnlyHashSet<Collider> staying)
            => this.OnReliableTriggerStayingChanged.Invoke(staying);

        protected override void OnReliableCollisionEnterCallback(Collider other)
            => this.OnReliableCollisionEnter.Invoke(other);
        protected override void OnReliableCollisionStayCallback(Collider other)
            => this.OnReliableCollisionStay.Invoke(other);
        protected override void OnReliableCollisionExitCallback(Collider other)
            => this.OnReliableCollisionExit.Invoke(other);
        protected override void OnReliableCollisionStayingChangedCallback(IReadOnlyHashSet<Collider> staying)
            => this.OnReliableCollisionStayingChanged.Invoke(staying);
    }
}
