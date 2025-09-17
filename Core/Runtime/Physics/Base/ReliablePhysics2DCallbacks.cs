using HisaCat.HUE.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HisaCat.HUE.PhysicsExtension
{
    public abstract class ReliablePhysics2DCallbacks : ReliablePhysicsCallbacksCore<Collider2D, Collision2D>
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
        private readonly static StaticBuffer<Collider2D> colliderBuffer = new((_) => new Collider2D[ColliderBufferCapacity]);

        protected sealed override ReliableCallbacks DelegateReliableCallbacks()
        {
            return new(
                trigger: new(
                    onEnter: this.OnReliableTrigger2DEnterCallback,
                    onStay: this.OnReliableTrigger2DStayCallback,
                    onExit: this.OnReliableTrigger2DExitCallback,
                    onStayingChanged: this.OnReliableTrigger2DStayingChangedCallback
                ),
                collision: new(
                    onEnter: this.OnReliableCollision2DEnterCallback,
                    onStay: this.OnReliableCollision2DStayCallback,
                    onExit: this.OnReliableCollision2DExitCallback,
                    onStayingChanged: this.OnReliableCollision2DStayingChangedCallback
                )
            );
        }

        #region Sealed override methods
        protected sealed override StaticBuffer<Collider2D> ColliderBuffer => colliderBuffer;
        protected sealed override bool IsColliderEnabled(Collider2D collider) => collider.enabled;
        protected sealed override Collider2D GetCollider(Collision2D collision) => collision.collider;
        #endregion Sealed override methods

        #region Unity Physics Callbacks
        protected virtual void OnTriggerEnter2D(Collider2D other) => this.HandleUnityTriggerEnter(other);
        protected virtual void OnTriggerExit2D(Collider2D other) => this.HandleUnityTriggerExit(other);
        protected virtual void OnCollisionEnter2D(Collision2D collision) => this.HandleUnityCollisionEnter(collision);
        protected virtual void OnCollisionExit2D(Collision2D collision) => this.HandleUnityCollisionExit(collision);
        #endregion Unity Physics Callbacks

        #region Reliable Physics Callbacks
        protected virtual void OnReliableTrigger2DEnterCallback(Collider2D other) { }
        protected virtual void OnReliableTrigger2DStayCallback(Collider2D other) { }
        protected virtual void OnReliableTrigger2DExitCallback(Collider2D other) { }
        protected virtual void OnReliableTrigger2DStayingChangedCallback(IReadOnlyHashSet<Collider2D> staying) { }

        protected virtual void OnReliableCollision2DEnterCallback(Collider2D other) { }
        protected virtual void OnReliableCollision2DStayCallback(Collider2D other) { }
        protected virtual void OnReliableCollision2DExitCallback(Collider2D other) { }
        protected virtual void OnReliableCollision2DStayingChangedCallback(IReadOnlyHashSet<Collider2D> staying) { }
        #endregion Reliable Physics Callbacks
    }
}
