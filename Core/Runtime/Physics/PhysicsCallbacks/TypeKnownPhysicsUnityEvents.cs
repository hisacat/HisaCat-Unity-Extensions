using HisaCat.HUE.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HisaCat.HUE.PhysicsExtension
{
    public abstract class TypeKnownPhysicsUnityEvents<TTarget> :
        TypeKnownPhysicsCallbacks<TTarget>
        where TTarget : Component
    {
        [System.Serializable] public class TargetTriggerEvent : UnityEngine.Events.UnityEvent<TTarget> { }
        public TargetTriggerEvent OnTargetTriggerEnter { get => this.m_OnTargetTriggerEnter; set => this.m_OnTargetTriggerEnter = value; }
        [SerializeField] private TargetTriggerEvent m_OnTargetTriggerEnter = new();
        public TargetTriggerEvent OnTargetTriggerStay { get => this.m_OnTargetTriggerStay; set => this.m_OnTargetTriggerStay = value; }
        [SerializeField] private TargetTriggerEvent m_OnTargetTriggerStay = new();
        public TargetTriggerEvent OnTargetTriggerExit { get => this.m_OnTargetTriggerExit; set => this.m_OnTargetTriggerExit = value; }
        [SerializeField] private TargetTriggerEvent m_OnTargetTriggerExit = new();
        [System.Serializable] public class TargetTriggerStayingChangedEvent : UnityEngine.Events.UnityEvent<IReadOnlyDictionary<TTarget, IReadOnlyHashSet<Collider>>> { }
        public TargetTriggerStayingChangedEvent OnTargetTriggerStayingChanged { get => this.m_OnTargetTriggerStayingChanged; set => this.m_OnTargetTriggerStayingChanged = value; }
        [SerializeField] private TargetTriggerStayingChangedEvent m_OnTargetTriggerStayingChanged = new();

        [System.Serializable] public class TargetCollisionEvent : UnityEngine.Events.UnityEvent<TTarget> { }
        public TargetCollisionEvent OnTargetCollisionEnter { get => this.m_OnTargetCollisionEnter; set => this.m_OnTargetCollisionEnter = value; }
        [SerializeField] private TargetCollisionEvent m_OnTargetCollisionEnter = new();
        public TargetCollisionEvent OnTargetCollisionStay { get => this.m_OnTargetCollisionStay; set => this.m_OnTargetCollisionStay = value; }
        [SerializeField] private TargetCollisionEvent m_OnTargetCollisionStay = new();
        public TargetCollisionEvent OnTargetCollisionExit { get => this.m_OnTargetCollisionExit; set => this.m_OnTargetCollisionExit = value; }
        [SerializeField] private TargetCollisionEvent m_OnTargetCollisionExit = new();
        [System.Serializable] public class TargetCollisionStayingChangedEvent : UnityEngine.Events.UnityEvent<IReadOnlyDictionary<TTarget, IReadOnlyHashSet<Collider>>> { }
        public TargetCollisionStayingChangedEvent OnTargetCollisionStayingChanged { get => this.m_OnTargetCollisionStayingChanged; set => this.m_OnTargetCollisionStayingChanged = value; }
        [SerializeField] private TargetCollisionStayingChangedEvent m_OnTargetCollisionStayingChanged = new();

        protected override void OnTargetTriggerEnterCallback(TTarget target)
            => this.OnTargetTriggerEnter.Invoke(target);
        protected override void OnTargetTriggerStayCallback(TTarget target)
            => this.OnTargetTriggerStay.Invoke(target);
        protected override void OnTargetTriggerExitCallback(TTarget target)
            => this.OnTargetTriggerExit.Invoke(target);
        protected override void OnTargetTriggerStayingChangedCallback(IReadOnlyDictionary<TTarget, IReadOnlyHashSet<Collider>> staying)
            => this.OnTargetTriggerStayingChanged.Invoke(staying);

        protected override void OnTargetCollisionEnterCallback(TTarget target)
            => this.OnTargetCollisionEnter.Invoke(target);
        protected override void OnTargetCollisionStayCallback(TTarget target)
            => this.OnTargetCollisionStay.Invoke(target);
        protected override void OnTargetCollisionExitCallback(TTarget target)
            => this.OnTargetCollisionExit.Invoke(target);
        protected override void OnTargetCollisionStayingChangedCallback(IReadOnlyDictionary<TTarget, IReadOnlyHashSet<Collider>> staying)
            => this.OnTargetCollisionStayingChanged.Invoke(staying);
    }
}
