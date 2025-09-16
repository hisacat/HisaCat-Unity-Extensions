using HisaCat.HUE.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HisaCat.HUE.PhysicsExtension
{
    public class ReliablePhysics2DCallbacks :
        ReliablePhysicsCallbacksCore<Collider2D, Collision2D>,
        IReliablePhysicsBridge<Collider2D, Collision2D>
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

        protected sealed override StaticBuffer<Collider2D> ColliderBuffer => colliderBuffer;
        protected sealed override bool IsColliderEnabled(Collider2D collider) => collider.enabled;
        protected sealed override Collider2D GetCollider(Collision2D collision) => collision.collider;

        protected override IReliablePhysicsBridge<Collider2D, Collision2D> CreateCore() => this;

        #region Unity Physics Callbacks
        protected virtual void OnTriggerEnter2D(Collider2D other)
            => this.OnUnityTriggerEnter(other);
        protected virtual void OnTriggerExit2D(Collider2D other)
            => this.OnUnityTriggerExit(other);
        protected virtual void OnCollisionEnter2D(Collision2D collision)
            => this.OnUnityCollisionEnter(collision);
        protected virtual void OnCollisionExit2D(Collision2D collision)
            => this.OnUnityCollisionExit(collision);
        #endregion Unity Physics Callbacks

        #region IReliablePhysicsBridge
        public void OnReliableTriggerEnterCallback_Internal(Collider2D other)
            => this.OnReliableTrigger2DEnterCallback(other);
        public void OnReliableTriggerStayCallback_Internal(Collider2D other)
            => this.OnReliableTrigger2DStayCallback(other);
        public void OnReliableTriggerExitCallback_Internal(Collider2D other)
            => this.OnReliableTrigger2DExitCallback(other);
        public void OnReliableTriggerStayingChangedCallback_Internal(IReadOnlyHashSet<Collider2D> staying)
            => this.OnReliableTrigger2DStayingChangedCallback(staying);

        public void OnReliableCollisionEnterCallback_Internal(Collider2D other)
            => this.OnReliableCollision2DEnterCallback(other);
        public void OnReliableCollisionStayCallback_Internal(Collider2D other)
            => this.OnReliableCollision2DStayCallback(other);
        public void OnReliableCollisionExitCallback_Internal(Collider2D other)
            => this.OnReliableCollision2DExitCallback(other);
        public void OnReliableCollisionStayingChangedCallback_Internal(IReadOnlyHashSet<Collider2D> staying)
            => this.OnReliableCollision2DStayingChangedCallback(staying);
        #endregion IReliablePhysicsBridge

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
