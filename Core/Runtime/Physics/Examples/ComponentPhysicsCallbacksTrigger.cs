using System.Collections.Generic;
using UnityEngine;

namespace HisaCat.HUE.PhysicsExtension
{
    public class ComponentPhysicsCallbacksTrigger : TypeKnownPhysicsCallbacks<Component>
    {
        public sealed override LayerMask TargetLayerMask => Physics.AllLayers;

        [System.Serializable] public class ComponentTriggerEvent : UnityEngine.Events.UnityEvent<Component> { }
        public ComponentTriggerEvent OnComponentTriggerEnter { get => this.m_OnComponentTriggerEnter; set => this.m_OnComponentTriggerEnter = value; }
        [SerializeField] private ComponentTriggerEvent m_OnComponentTriggerEnter = null;
        public ComponentTriggerEvent OnComponentTriggerStay { get => this.m_OnComponentTriggerStay; set => this.m_OnComponentTriggerStay = value; }
        [SerializeField] private ComponentTriggerEvent m_OnComponentTriggerStay = null;
        public ComponentTriggerEvent OnComponentTriggerExit { get => this.m_OnComponentTriggerExit; set => this.m_OnComponentTriggerExit = value; }
        [SerializeField] private ComponentTriggerEvent m_OnComponentTriggerExit = null;

        [System.Serializable] public class ComponentCollisionEvent : UnityEngine.Events.UnityEvent<Component> { }
        public ComponentCollisionEvent OnComponentCollisionEnter { get => this.m_OnComponentCollisionEnter; set => this.m_OnComponentCollisionEnter = value; }
        [SerializeField] private ComponentCollisionEvent m_OnComponentCollisionEnter = null;
        public ComponentCollisionEvent OnComponentCollisionStay { get => this.m_OnComponentCollisionStay; set => this.m_OnComponentCollisionStay = value; }
        [SerializeField] private ComponentCollisionEvent m_OnComponentCollisionStay = null;
        public ComponentCollisionEvent OnComponentCollisionExit { get => this.m_OnComponentCollisionExit; set => this.m_OnComponentCollisionExit = value; }
        [SerializeField] private ComponentCollisionEvent m_OnComponentCollisionExit = null;

        protected override void OnTargetTriggerEnterCallback(Component target)
            => this.OnComponentTriggerEnter.Invoke(target);
        protected override void OnTargetTriggerStayCallback(Component target)
            => this.OnComponentTriggerStay.Invoke(target);
        protected override void OnTargetTriggerExitCallback(Component target)
            => this.OnComponentTriggerExit.Invoke(target);

        protected override void OnTargetCollisionEnterCallback(Component target)
            => this.OnComponentCollisionEnter.Invoke(target);
        protected override void OnTargetCollisionStayCallback(Component target)
            => this.OnComponentCollisionStay.Invoke(target);
        protected override void OnTargetCollisionExitCallback(Component target)
            => this.OnComponentCollisionExit.Invoke(target);
    }
}
