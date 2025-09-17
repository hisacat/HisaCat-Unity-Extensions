using HisaCat.HUE.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HisaCat.HUE.PhysicsExtension
{
    public abstract class ReliablePhysicsCallbacks : ReliablePhysicsCallbacksCore<Collider, Collision>
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

        protected sealed override ReliableCallbacks DelegateReliableCallbacks()
        {
            return new(
                trigger: new(
                    onEnter: this.OnReliableTriggerEnterCallback,
                    onStay: this.OnReliableTriggerStayCallback,
                    onExit: this.OnReliableTriggerExitCallback,
                    onStayingChanged: this.OnReliableTriggerStayingChangedCallback
                ),
                collision: new(
                    onEnter: this.OnReliableCollisionEnterCallback,
                    onStay: this.OnReliableCollisionStayCallback,
                    onExit: this.OnReliableCollisionExitCallback,
                    onStayingChanged: this.OnReliableCollisionStayingChangedCallback
                )
            );
        }

        #region Sealed override methods
        protected sealed override StaticBuffer<Collider> ColliderBuffer => colliderBuffer;
        protected sealed override bool IsColliderEnabled(Collider collider) => collider.enabled;
        protected sealed override Collider GetCollider(Collision collision) => collision.collider;
        #endregion Sealed override methods

        #region Handle Unity Physics Callbacks
        protected virtual void OnTriggerEnter(Collider other) => this.HandleUnityTriggerEnter(other);
        protected virtual void OnTriggerExit(Collider other) => this.HandleUnityTriggerExit(other);
        protected virtual void OnCollisionEnter(Collision collision) => this.HandleUnityCollisionEnter(collision);
        protected virtual void OnCollisionExit(Collision collision) => this.HandleUnityCollisionExit(collision);
        #endregion Handle Unity Physics Callbacks

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
