using HisaCat.HUE.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HisaCat.HUE.PhysicsExtension
{
    public abstract class TypeKnownPhysics2DEvents<TTarget> :
        TypeKnownPhysics2DCallbacks<TTarget>
        where TTarget : Component
    {
        public delegate void TargetTrigger2DDelegate(TTarget target);
        public event TargetTrigger2DDelegate OnTargetTriggerEnter2D = null;
        public event TargetTrigger2DDelegate OnTargetTriggerStay2D = null;
        public event TargetTrigger2DDelegate OnTargetTriggerExit2D = null;
        public delegate void TargetTriggerStayingChanged2DDelegate(IReadOnlyDictionary<TTarget, IReadOnlyHashSet<Collider2D>> staying);
        public event TargetTriggerStayingChanged2DDelegate OnTargetTriggerStayingChanged2D = null;

        public delegate void TargetCollision2DDelegate(TTarget target);
        public event TargetCollision2DDelegate OnTargetCollisionEnter2D = null;
        public event TargetCollision2DDelegate OnTargetCollisionStay2D = null;
        public event TargetCollision2DDelegate OnTargetCollisionExit2D = null;
        public delegate void TargetCollisionStayingChanged2DDelegate(IReadOnlyDictionary<TTarget, IReadOnlyHashSet<Collider2D>> staying);
        public event TargetCollisionStayingChanged2DDelegate OnTargetCollisionStayingChanged2D = null;

        protected override void OnTargetTriggerEnter2DCallback(TTarget target)
            => this.OnTargetTriggerEnter2D?.Invoke(target);
        protected override void OnTargetTriggerStay2DCallback(TTarget target)
            => this.OnTargetTriggerStay2D?.Invoke(target);
        protected override void OnTargetTriggerExit2DCallback(TTarget target)
            => this.OnTargetTriggerExit2D?.Invoke(target);
        protected override void OnTargetTriggerStayingChanged2DCallback(IReadOnlyDictionary<TTarget, IReadOnlyHashSet<Collider2D>> staying)
            => this.OnTargetTriggerStayingChanged2D?.Invoke(staying);

        protected override void OnTargetCollisionEnter2DCallback(TTarget target)
            => this.OnTargetCollisionEnter2D?.Invoke(target);
        protected override void OnTargetCollisionStay2DCallback(TTarget target)
            => this.OnTargetCollisionStay2D?.Invoke(target);
        protected override void OnTargetCollisionExit2DCallback(TTarget target)
            => this.OnTargetCollisionExit2D?.Invoke(target);
        protected override void OnTargetCollisionStayingChanged2DCallback(IReadOnlyDictionary<TTarget, IReadOnlyHashSet<Collider2D>> staying)
            => this.OnTargetCollisionStayingChanged2D?.Invoke(staying);
    }
}
