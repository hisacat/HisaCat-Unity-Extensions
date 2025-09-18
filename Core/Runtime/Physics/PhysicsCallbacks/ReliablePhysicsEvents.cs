using HisaCat.HUE.Collections;
using UnityEngine;

namespace HisaCat.HUE.PhysicsExtension
{
    [DefaultExecutionOrder(int.MinValue)]
    public class ReliablePhysicsEvents : ReliablePhysicsCallbacks
    {
        public delegate void ReliableTriggerDelegate(Collider other);
        public event ReliableTriggerDelegate OnReliableTriggerEnter = null;
        public event ReliableTriggerDelegate OnReliableTriggerStay = null;
        public event ReliableTriggerDelegate OnReliableTriggerExit = null;
        public delegate void ReliableTriggerStayingChangedDelegate(IReadOnlyHashSet<Collider> staying);
        public event ReliableTriggerStayingChangedDelegate OnReliableTriggerStayingChanged = null;

        public delegate void ReliableCollisionDelegate(Collider other);
        public event ReliableCollisionDelegate OnReliableCollisionEnter = null;
        public event ReliableCollisionDelegate OnReliableCollisionStay = null;
        public event ReliableCollisionDelegate OnReliableCollisionExit = null;
        public delegate void ReliableCollisionStayingChangedDelegate(IReadOnlyHashSet<Collider> staying);
        public event ReliableCollisionStayingChangedDelegate OnReliableCollisionStayingChanged = null;

        protected override void OnReliableTriggerEnterCallback(Collider other)
            => this.OnReliableTriggerEnter?.Invoke(other);
        protected override void OnReliableTriggerStayCallback(Collider other)
            => this.OnReliableTriggerStay?.Invoke(other);
        protected override void OnReliableTriggerExitCallback(Collider other)
            => this.OnReliableTriggerExit?.Invoke(other);
        protected override void OnReliableTriggerStayingChangedCallback(IReadOnlyHashSet<Collider> staying)
            => this.OnReliableTriggerStayingChanged?.Invoke(staying);

        protected override void OnReliableCollisionEnterCallback(Collider other)
            => this.OnReliableCollisionEnter?.Invoke(other);
        protected override void OnReliableCollisionStayCallback(Collider other)
            => this.OnReliableCollisionStay?.Invoke(other);
        protected override void OnReliableCollisionExitCallback(Collider other)
            => this.OnReliableCollisionExit?.Invoke(other);
        protected override void OnReliableCollisionStayingChangedCallback(IReadOnlyHashSet<Collider> staying)
            => this.OnReliableCollisionStayingChanged?.Invoke(staying);
    }
}
