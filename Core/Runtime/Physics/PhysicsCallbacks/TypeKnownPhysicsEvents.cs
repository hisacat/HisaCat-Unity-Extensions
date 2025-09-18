using HisaCat.HUE.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HisaCat.HUE.PhysicsExtension
{
    public abstract class TypeKnownPhysicsEvents<TTarget> :
        TypeKnownPhysicsCallbacks<TTarget>
        where TTarget : Component
    {
        public delegate void TargetTriggerDelegate(TTarget target);
        public event TargetTriggerDelegate OnTargetTriggerEnter = null;
        public event TargetTriggerDelegate OnTargetTriggerStay = null;
        public event TargetTriggerDelegate OnTargetTriggerExit = null;
        public delegate void TargetTriggerStayingChangedDelegate(IReadOnlyDictionary<TTarget, IReadOnlyHashSet<Collider>> staying);
        public event TargetTriggerStayingChangedDelegate OnTargetTriggerStayingChanged = null;

        public delegate void TargetCollisionDelegate(TTarget target);
        public event TargetCollisionDelegate OnTargetCollisionEnter = null;
        public event TargetCollisionDelegate OnTargetCollisionStay = null;
        public event TargetCollisionDelegate OnTargetCollisionExit = null;
        public delegate void TargetCollisionStayingChangedDelegate(IReadOnlyDictionary<TTarget, IReadOnlyHashSet<Collider>> staying);
        public event TargetCollisionStayingChangedDelegate OnTargetCollisionStayingChanged = null;

        protected override void OnTargetTriggerEnterCallback(TTarget target)
            => this.OnTargetTriggerEnter?.Invoke(target);
        protected override void OnTargetTriggerStayCallback(TTarget target)
            => this.OnTargetTriggerStay?.Invoke(target);
        protected override void OnTargetTriggerExitCallback(TTarget target)
            => this.OnTargetTriggerExit?.Invoke(target);
        protected override void OnTargetTriggerStayingChangedCallback(IReadOnlyDictionary<TTarget, IReadOnlyHashSet<Collider>> staying)
            => this.OnTargetTriggerStayingChanged?.Invoke(staying);

        protected override void OnTargetCollisionEnterCallback(TTarget target)
            => this.OnTargetCollisionEnter?.Invoke(target);
        protected override void OnTargetCollisionStayCallback(TTarget target)
            => this.OnTargetCollisionStay?.Invoke(target);
        protected override void OnTargetCollisionExitCallback(TTarget target)
            => this.OnTargetCollisionExit?.Invoke(target);
        protected override void OnTargetCollisionStayingChangedCallback(IReadOnlyDictionary<TTarget, IReadOnlyHashSet<Collider>> staying)
            => this.OnTargetCollisionStayingChanged?.Invoke(staying);
    }
}
