using HisaCat.HUE.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HisaCat.HUE.PhysicsExtension
{
    public abstract class TypeKnownPhysics2DUnityEvents<TTarget> :
        TypeKnownPhysics2DCallbacks<TTarget>
        where TTarget : Component
    {
        [System.Serializable] public class TargetTrigger2DEvent : UnityEngine.Events.UnityEvent<TTarget> { }
        public TargetTrigger2DEvent OnTargetTriggerEnter2D { get => this.m_OnTargetTriggerEnter2D; set => this.m_OnTargetTriggerEnter2D = value; }
        [SerializeField] private TargetTrigger2DEvent m_OnTargetTriggerEnter2D = new();
        public TargetTrigger2DEvent OnTargetTriggerStay2D { get => this.m_OnTargetTriggerStay2D; set => this.m_OnTargetTriggerStay2D = value; }
        [SerializeField] private TargetTrigger2DEvent m_OnTargetTriggerStay2D = new();
        public TargetTrigger2DEvent OnTargetTriggerExit2D { get => this.m_OnTargetTriggerExit2D; set => this.m_OnTargetTriggerExit2D = value; }
        [SerializeField] private TargetTrigger2DEvent m_OnTargetTriggerExit2D = new();
        [System.Serializable] public class TargetTriggerStayingChanged2DEvent : UnityEngine.Events.UnityEvent<IReadOnlyDictionary<TTarget, IReadOnlyHashSet<Collider2D>>> { }
        public TargetTriggerStayingChanged2DEvent OnTargetTriggerStayingChanged2D { get => this.m_OnTargetTriggerStayingChanged2D; set => this.m_OnTargetTriggerStayingChanged2D = value; }
        [SerializeField] private TargetTriggerStayingChanged2DEvent m_OnTargetTriggerStayingChanged2D = new();

        [System.Serializable] public class TargetCollision2DEvent : UnityEngine.Events.UnityEvent<TTarget> { }
        public TargetCollision2DEvent OnTargetCollisionEnter2D { get => this.m_OnTargetCollisionEnter2D; set => this.m_OnTargetCollisionEnter2D = value; }
        [SerializeField] private TargetCollision2DEvent m_OnTargetCollisionEnter2D = new();
        public TargetCollision2DEvent OnTargetCollisionStay2D { get => this.m_OnTargetCollisionStay2D; set => this.m_OnTargetCollisionStay2D = value; }
        [SerializeField] private TargetCollision2DEvent m_OnTargetCollisionStay2D = new();
        public TargetCollision2DEvent OnTargetCollisionExit2D { get => this.m_OnTargetCollisionExit2D; set => this.m_OnTargetCollisionExit2D = value; }
        [SerializeField] private TargetCollision2DEvent m_OnTargetCollisionExit2D = new();
        [System.Serializable] public class TargetCollisionStayingChanged2DEvent : UnityEngine.Events.UnityEvent<IReadOnlyDictionary<TTarget, IReadOnlyHashSet<Collider2D>>> { }
        public TargetCollisionStayingChanged2DEvent OnTargetCollisionStayingChanged2D { get => this.m_OnTargetCollisionStayingChanged2D; set => this.m_OnTargetCollisionStayingChanged2D = value; }
        [SerializeField] private TargetCollisionStayingChanged2DEvent m_OnTargetCollisionStayingChanged2D = new();

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
