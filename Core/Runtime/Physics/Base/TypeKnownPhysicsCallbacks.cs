
using HisaCat.HUE.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HisaCat.HUE.PhysicsExtension
{
    public abstract class TypeKnownPhysicsCallbacks<TTarget> :
        TypeKnownPhysicsCallbacksCore<TTarget, Collider, Collision>
        where TTarget : Component
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

        protected override TypeKnownCallbacks DelegateTypeKnownCallbacks()
        {
            return new(
                trigger: new(
                    onEnter: this.OnTargetTriggerEnterCallback,
                    onStay: this.OnTargetTriggerStayCallback,
                    onExit: this.OnTargetTriggerExitCallback,
                    onStayingChanged: this.OnTargetTriggerStayingChangedCallback
                ),
                collision: new(
                    onEnter: this.OnTargetCollisionEnterCallback,
                    onStay: this.OnTargetCollisionStayCallback,
                    onExit: this.OnTargetCollisionExitCallback,
                    onStayingChanged: this.OnTargetCollisionStayingChangedCallback
                )
            );
        }

        private const int ColliderBufferCapacity = 1024;
        private readonly static StaticBuffer<Collider> colliderBuffer = new((_) => new Collider[ColliderBufferCapacity]);

        #region Sealed override methods
        protected sealed override StaticBuffer<Collider> ColliderBuffer => colliderBuffer;
        protected sealed override bool IsColliderEnabled(Collider collider) => collider.enabled;
        protected sealed override Collider GetCollider(Collision collision) => collision.collider;

        protected sealed override GameObject GetColliderGameObject(Collider collider) => collider.gameObject;
        #endregion Sealed override methods

        #region Handle Unity Physics Callbacks
        protected virtual void OnTriggerEnter(Collider other) => this.HandleUnityTriggerEnter(other);
        protected virtual void OnTriggerExit(Collider other) => this.HandleUnityTriggerExit(other);
        protected virtual void OnCollisionEnter(Collision collision) => this.HandleUnityCollisionEnter(collision);
        protected virtual void OnCollisionExit(Collision collision) => this.HandleUnityCollisionExit(collision);
        #endregion Handle Unity Physics Callbacks


        #region Target Physics Callbacks
        protected virtual void OnTargetTriggerEnterCallback(TTarget target) { }
        protected virtual void OnTargetTriggerStayCallback(TTarget target) { }
        protected virtual void OnTargetTriggerExitCallback(TTarget target) { }
        protected virtual void OnTargetTriggerStayingChangedCallback(IReadOnlyDictionary<TTarget, IReadOnlyHashSet<Collider>> staying) { }

        protected virtual void OnTargetCollisionStayCallback(TTarget target) { }
        protected virtual void OnTargetCollisionEnterCallback(TTarget target) { }
        protected virtual void OnTargetCollisionExitCallback(TTarget target) { }
        protected virtual void OnTargetCollisionStayingChangedCallback(IReadOnlyDictionary<TTarget, IReadOnlyHashSet<Collider>> staying) { }
        #endregion Target Physics Callbacks
    }
}
