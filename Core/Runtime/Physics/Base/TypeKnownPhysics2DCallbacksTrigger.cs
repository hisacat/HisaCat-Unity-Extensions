using HisaCat.HUE.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HisaCat.HUE.PhysicsExtension
{
    public abstract class TypeKnownPhysics2DCallbacksTrigger<TTarget> :
        TypeKnownPhysics2DCallbacks<TTarget>
        where TTarget : Component
    {
        [System.Serializable] public class TargetTriggerEvent : UnityEngine.Events.UnityEvent<TTarget> { }
        public TargetTriggerEvent OnTargetTriggerEnter2D { get => this.m_OnTargetTriggerEnter2D; set => this.m_OnTargetTriggerEnter2D = value; }
        [SerializeField] private TargetTriggerEvent m_OnTargetTriggerEnter2D = null;
        public TargetTriggerEvent OnTargetTriggerStay2D { get => this.m_OnTargetTriggerStay2D; set => this.m_OnTargetTriggerStay2D = value; }
        [SerializeField] private TargetTriggerEvent m_OnTargetTriggerStay2D = null;
        public TargetTriggerEvent OnTargetTriggerExit2D { get => this.m_OnTargetTriggerExit2D; set => this.m_OnTargetTriggerExit2D = value; }
        [SerializeField] private TargetTriggerEvent m_OnTargetTriggerExit2D = null;
        [System.Serializable] public class TargetTriggerStayingChangedEvent : UnityEngine.Events.UnityEvent<IReadOnlyDictionary<TTarget, IReadOnlyHashSet<Collider2D>>> { }
        public TargetTriggerStayingChangedEvent OnTargetTriggerStayingChanged2D { get => this.m_OnTargetTriggerStayingChanged2D; set => this.m_OnTargetTriggerStayingChanged2D = value; }
        [SerializeField] private TargetTriggerStayingChangedEvent m_OnTargetTriggerStayingChanged2D = null;

        [System.Serializable] public class TargetCollisionEvent : UnityEngine.Events.UnityEvent<TTarget> { }
        public TargetCollisionEvent OnTargetCollisionEnter2D { get => this.m_OnTargetCollisionEnter2D; set => this.m_OnTargetCollisionEnter2D = value; }
        [SerializeField] private TargetCollisionEvent m_OnTargetCollisionEnter2D = null;
        public TargetCollisionEvent OnTargetCollisionStay2D { get => this.m_OnTargetCollisionStay2D; set => this.m_OnTargetCollisionStay2D = value; }
        [SerializeField] private TargetCollisionEvent m_OnTargetCollisionStay2D = null;
        public TargetCollisionEvent OnTargetCollisionExit2D { get => this.m_OnTargetCollisionExit2D; set => this.m_OnTargetCollisionExit2D = value; }
        [SerializeField] private TargetCollisionEvent m_OnTargetCollisionExit2D = null;
        [System.Serializable] public class TargetCollisionStayingChangedEvent : UnityEngine.Events.UnityEvent<IReadOnlyDictionary<TTarget, IReadOnlyHashSet<Collider2D>>> { }
        public TargetCollisionStayingChangedEvent OnTargetCollisionStayingChanged2D { get => this.m_OnTargetCollisionStayingChanged2D; set => this.m_OnTargetCollisionStayingChanged2D = value; }
        [SerializeField] private TargetCollisionStayingChangedEvent m_OnTargetCollisionStayingChanged2D = null;

        protected override void OnTargetTriggerEnter2DCallback(TTarget target)
            => this.OnTargetTriggerEnter2D.Invoke(target);
        protected override void OnTargetTriggerStay2DCallback(TTarget target)
            => this.OnTargetTriggerStay2D.Invoke(target);
        protected override void OnTargetTriggerExit2DCallback(TTarget target)
            => this.OnTargetTriggerExit2D.Invoke(target);
        protected override void OnTargetTriggerStayingChanged2DCallback(IReadOnlyDictionary<TTarget, IReadOnlyHashSet<Collider2D>> staying)
            => this.OnTargetTriggerStayingChanged2D.Invoke(staying);

        protected override void OnTargetCollisionEnter2DCallback(TTarget target)
            => this.OnTargetCollisionEnter2D.Invoke(target);
        protected override void OnTargetCollisionStay2DCallback(TTarget target)
            => this.OnTargetCollisionStay2D.Invoke(target);
        protected override void OnTargetCollisionExit2DCallback(TTarget target)
            => this.OnTargetCollisionExit2D.Invoke(target);
        protected override void OnTargetCollisionStayingChanged2DCallback(IReadOnlyDictionary<TTarget, IReadOnlyHashSet<Collider2D>> staying)
            => this.OnTargetCollisionStayingChanged2D.Invoke(staying);
    }
}
