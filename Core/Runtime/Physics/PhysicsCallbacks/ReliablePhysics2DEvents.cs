using HisaCat.HUE.Collections;
using UnityEngine;

namespace HisaCat.HUE.PhysicsExtension
{
    [DefaultExecutionOrder(int.MinValue)]
    public class ReliablePhysics2DEvents : ReliablePhysics2DCallbacks
    {
        public delegate void ReliableTrigger2DDelegate(Collider2D other);
        public event ReliableTrigger2DDelegate OnReliableTrigger2DEnter = null;
        public event ReliableTrigger2DDelegate OnReliableTrigger2DStay = null;
        public event ReliableTrigger2DDelegate OnReliableTrigger2DExit = null;
        public delegate void ReliableTriggerStayingChanged2DDelegate(IReadOnlyHashSet<Collider2D> staying);
        public event ReliableTriggerStayingChanged2DDelegate OnReliableTriggerStayingChanged2D = null;

        public delegate void ReliableCollision2DDelegate(Collider2D other);
        public event ReliableCollision2DDelegate OnReliableCollision2DEnter = null;
        public event ReliableCollision2DDelegate OnReliableCollision2DStay = null;
        public event ReliableCollision2DDelegate OnReliableCollision2DExit = null;
        public delegate void ReliableCollisionStayingChanged2DDelegate(IReadOnlyHashSet<Collider2D> staying);
        public event ReliableCollisionStayingChanged2DDelegate OnReliableCollisionStayingChanged2D = null;

        protected override void OnReliableTrigger2DEnterCallback(Collider2D other)
            => this.OnReliableTrigger2DEnter?.Invoke(other);
        protected override void OnReliableTrigger2DStayCallback(Collider2D other)
            => this.OnReliableTrigger2DStay?.Invoke(other);
        protected override void OnReliableTrigger2DExitCallback(Collider2D other)
            => this.OnReliableTrigger2DExit?.Invoke(other);
        protected override void OnReliableTriggerStayingChanged2DCallback(IReadOnlyHashSet<Collider2D> staying)
            => this.OnReliableTriggerStayingChanged2D?.Invoke(staying);

        protected override void OnReliableCollision2DEnterCallback(Collider2D other)
            => this.OnReliableCollision2DEnter?.Invoke(other);
        protected override void OnReliableCollision2DStayCallback(Collider2D other)
            => this.OnReliableCollision2DStay?.Invoke(other);
        protected override void OnReliableCollision2DExitCallback(Collider2D other)
            => this.OnReliableCollision2DExit?.Invoke(other);
        protected override void OnReliableCollisionStayingChanged2DCallback(IReadOnlyHashSet<Collider2D> staying)
            => this.OnReliableCollisionStayingChanged2D?.Invoke(staying);
    }
}
