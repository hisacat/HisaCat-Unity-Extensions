using HisaCat.HUE.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HisaCat.HUE.PhysicsExtension
{
    public class ReliablePhysicsCallbacks :
        ReliablePhysicsCallbacksCore<Collider, Collision>,
        IReliablePhysicsBridge<Collider, Collision>
    {
#if UNITY_EDITOR
#pragma warning disable IDE0051
        [UnityEditor.InitializeOnEnterPlayMode]
        private static void OnEnterPlaymodeInEditor(UnityEditor.EnterPlayModeOptions options)
        {
            if (options.HasFlag(UnityEditor.EnterPlayModeOptions.DisableDomainReload))
            {
                colliderBuffer.Initialize();
            }
        }
#pragma warning restore IDE0051
#endif
        private const int ColliderBufferCapacity = 1024;
        private readonly static StaticBuffer<Collider> colliderBuffer = new((_) => new Collider[ColliderBufferCapacity]);

        protected sealed override StaticBuffer<Collider> ColliderBuffer => colliderBuffer;
        protected sealed override bool IsColliderEnabled(Collider collider) => collider.enabled;
        protected sealed override Collider GetCollider(Collision collision) => collision.collider;

        protected override IReliablePhysicsBridge<Collider, Collision> CreateCore() => this;

        #region Unity Physics Callbacks
        protected virtual void OnTriggerEnter(Collider other)
            => ((IUnityPhysicsHandler<Collider, Collision>)this).HandleTriggerEnter(other);
        protected virtual void OnTriggerExit(Collider other)
            => ((IUnityPhysicsHandler<Collider, Collision>)this).HandleTriggerExit(other);
        protected virtual void OnCollisionEnter(Collision collision)
            => ((IUnityPhysicsHandler<Collider, Collision>)this).HandleCollisionEnter(collision);
        protected virtual void OnCollisionExit(Collision collision)
            => ((IUnityPhysicsHandler<Collider, Collision>)this).HandleCollisionExit(collision);
        #endregion Unity Physics Callbacks

        #region IReliablePhysicsBridge
        public void OnReliableTriggerEnterCallback_Internal(Collider other)
            => this.OnReliableTriggerEnterCallback(other);
        public void OnReliableTriggerStayCallback_Internal(Collider other)
            => this.OnReliableTriggerStayCallback(other);
        public void OnReliableTriggerExitCallback_Internal(Collider other)
            => this.OnReliableTriggerExitCallback(other);
        public void OnReliableTriggerStayingChangedCallback_Internal(IReadOnlyHashSet<Collider> staying)
            => this.OnReliableTriggerStayingChangedCallback(staying);

        public void OnReliableCollisionEnterCallback_Internal(Collider other)
            => this.OnReliableCollisionEnterCallback(other);
        public void OnReliableCollisionStayCallback_Internal(Collider other)
            => this.OnReliableCollisionStayCallback(other);
        public void OnReliableCollisionExitCallback_Internal(Collider other)
            => this.OnReliableCollisionExitCallback(other);
        public void OnReliableCollisionStayingChangedCallback_Internal(IReadOnlyHashSet<Collider> staying)
            => this.OnReliableCollisionStayingChangedCallback(staying);
        #endregion IReliablePhysicsBridge

        #region Reliable Physics Callbacks
        protected virtual void OnReliableTriggerEnterCallback(Collider other) { }
        protected virtual void OnReliableTriggerStayCallback(Collider other) { }
        protected virtual void OnReliableTriggerExitCallback(Collider other) { }
        protected virtual void OnReliableTriggerStayingChangedCallback(IReadOnlyHashSet<Collider> staying) { }

        protected virtual void OnReliableCollisionEnterCallback(Collider other) { }
        protected virtual void OnReliableCollisionStayCallback(Collider other) { }
        protected virtual void OnReliableCollisionExitCallback(Collider other) { }
        protected virtual void OnReliableCollisionStayingChangedCallback(IReadOnlyHashSet<Collider> staying) { }
        #endregion Reliable Physics Callbacks
    }
}
