
using HisaCat.HUE.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HisaCat.HUE.PhysicsExtension
{
    internal static class TypeKnownPhysics2DCallbacksBuffer
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
        internal readonly static StaticBuffer<Collider2D> colliderBuffer = new((_) => new Collider2D[ColliderBufferCapacity]);
    }

    public abstract class TypeKnownPhysics2DCallbacks<TTarget> :
        TypeKnownPhysicsCallbacksCore<TTarget, Collider2D, Collision2D>
        where TTarget : Component
    {
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

        #region Sealed override methods
        protected sealed override StaticBuffer<Collider2D> ColliderBuffer => TypeKnownPhysics2DCallbacksBuffer.colliderBuffer;
        protected sealed override bool IsColliderEnabled(Collider2D collider) => collider.enabled;
        protected sealed override Collider2D GetCollider(Collision2D collision) => collision.collider;

        protected sealed override GameObject GetColliderGameObject(Collider2D collider) => collider.gameObject;
        #endregion Sealed override methods

        #region Handle Unity Physics Callbacks
        protected virtual void OnTriggerEnter(Collider2D other) => this.HandleUnityTriggerEnter(other);
        protected virtual void OnTriggerExit(Collider2D other) => this.HandleUnityTriggerExit(other);
        protected virtual void OnCollisionEnter(Collision2D collision) => this.HandleUnityCollisionEnter(collision);
        protected virtual void OnCollisionExit(Collision2D collision) => this.HandleUnityCollisionExit(collision);
        #endregion Handle Unity Physics Callbacks


        #region Target Physics Callbacks
        protected virtual void OnTargetTriggerEnterCallback(TTarget target) { }
        protected virtual void OnTargetTriggerStayCallback(TTarget target) { }
        protected virtual void OnTargetTriggerExitCallback(TTarget target) { }
        protected virtual void OnTargetTriggerStayingChangedCallback(IReadOnlyDictionary<TTarget, IReadOnlyHashSet<Collider2D>> staying) { }

        protected virtual void OnTargetCollisionStayCallback(TTarget target) { }
        protected virtual void OnTargetCollisionEnterCallback(TTarget target) { }
        protected virtual void OnTargetCollisionExitCallback(TTarget target) { }
        protected virtual void OnTargetCollisionStayingChangedCallback(IReadOnlyDictionary<TTarget, IReadOnlyHashSet<Collider2D>> staying) { }
        #endregion Target Physics Callbacks
    }
}
