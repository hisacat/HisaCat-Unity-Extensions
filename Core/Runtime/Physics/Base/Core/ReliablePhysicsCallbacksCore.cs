using HisaCat.HUE.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HisaCat.HUE.PhysicsExtension
{
    /// <summary>
    /// ReliablePhysicsCallbacks ensures that physics enter/stay/exit events<br/>
    /// are invoked consistently, even when Unity would normally skip them.<br/>
    /// It also manages the collection of currently colliding objects.
    /// <para>
    /// Unity's default behavior:
    /// - When the GameObject itself or the collision target is <b>Disabled</b> or <b>Destroyed</b>,
    ///   OnTriggerExit / OnCollisionExit events are not invoked.
    /// </para>
    /// <para>
    /// This class compensates for these missing callbacks,<br/>
    /// ensuring that Exit callbacks are properly invoked even when objects are<br/>
    /// disabled or destroyed.
    /// </para>
    /// <para>
    /// Guaranteed callback invocation order:<br/>
    /// - On Enter: Enter → StayingChanged<br/>
    /// - On Exit: StayingChanged → Exit
    /// </para>
    /// </summary>
    public abstract class ReliablePhysicsCallbacksCore<TCollider, TCollision> : MonoBehaviour
        where TCollider : Component where TCollision : class
    {
        #region Abstract Methods
        /// <summary>
        /// Provides the delegated callbacks used by Reliable Physics Callbacks.
        /// </summary>
        protected abstract ReliableCallbacks DelegateReliableCallbacks();
        /// <summary>
        /// Returns whether the specified collider is enabled.
        /// </summary>
        protected abstract bool IsColliderEnabled(TCollider collider);
        /// <summary>
        /// Extracts the collider from the given collision.
        /// </summary>
        protected abstract TCollider GetCollider(TCollision collision);
        /// <summary>
        /// Gets the static collider buffer.
        /// <remarks>
        /// Generic classes cannot use the InitializeOnEnterPlayMode attribute,<br/>
        /// this static field must be defined in derived classes to support environments<br/>
        /// where domain reload is disabled.
        /// </remarks>
        /// </summary>
        protected abstract StaticBuffer<TCollider> ColliderBuffer { get; }
        #endregion Abstract Methods

        public IReadOnlyHashSet<TCollider> TriggerStayingColliders { get; private set; } = null;
        private HashSet<TCollider> triggerStayingColliders = null;
        public IReadOnlyHashSet<TCollider> CollisionStayingColliders { get; private set; } = null;
        private HashSet<TCollider> collisionStayingColliders = null;

        private ReliableCallbacks reliableCallbacks = null;
        protected virtual void Awake()
        {
            this.TriggerStayingColliders = (this.triggerStayingColliders = new()).AsReadOnly();
            this.CollisionStayingColliders = (this.collisionStayingColliders = new()).AsReadOnly();

            this.reliableCallbacks = this.DelegateReliableCallbacks();
        }

        public class ReliableCallbacks
        {
            public delegate void ColliderDelegate(TCollider other);
            public delegate void ReadOnlyCollidersDelegate(IReadOnlyHashSet<TCollider> staying);

            public readonly Callbacks Trigger;
            public readonly Callbacks Collision;
            public class Callbacks
            {
                public readonly ColliderDelegate OnEnter;
                public readonly ColliderDelegate OnStay;
                public readonly ColliderDelegate OnExit;
                public readonly ReadOnlyCollidersDelegate OnStayingChanged;

                public Callbacks(
                    ColliderDelegate onEnter, ColliderDelegate onStay, ColliderDelegate onExit,
                    ReadOnlyCollidersDelegate onStayingChanged)
                {
                    this.OnEnter = onEnter;
                    this.OnStay = onStay;
                    this.OnExit = onExit;
                    this.OnStayingChanged = onStayingChanged;
                }
            }

            public ReliableCallbacks(Callbacks trigger, Callbacks collision)
            {
                this.Trigger = trigger;
                this.Collision = collision;
            }
        }

        protected virtual void OnDisable() => this.ForceExitAll();
        protected virtual void OnDestroy() => this.ForceExitAll();
        private void ForceExitAll()
        {
            Process(this.triggerStayingColliders, this.TriggerStayingColliders, ColliderBuffer, this.reliableCallbacks.Trigger);
            Process(this.collisionStayingColliders, this.CollisionStayingColliders, ColliderBuffer, this.reliableCallbacks.Collision);
            static void Process(
                HashSet<TCollider> staying, IReadOnlyHashSet<TCollider> readOnlyStaying,
                StaticBuffer<TCollider> buffer, ReliableCallbacks.Callbacks callbacks)
            {
                var removeCount = 0;
                var removeBuffer = buffer;
                foreach (var stay in staying) removeBuffer.AddItemSafely(ref removeCount, stay);

                if (removeCount > 0)
                {
                    for (int i = 0; i < removeCount; i++)
                    {
                        var stay = removeBuffer.Buffer[i];
                        staying.Remove(stay);

                        // Always Fire staying changed callback first.
                        callbacks.OnStayingChanged(readOnlyStaying);
                        callbacks.OnExit(stay);
                    }
                }
                removeBuffer.ClearBuffer(removeCount);
            }
        }

        protected virtual void FixedUpdate()
        {
            Process(this.triggerStayingColliders, this.TriggerStayingColliders, ColliderBuffer, this.IsColliderEnabled, this.reliableCallbacks.Trigger);
            Process(this.collisionStayingColliders, this.CollisionStayingColliders, ColliderBuffer, this.IsColliderEnabled, this.reliableCallbacks.Collision);
            static void Process(
                HashSet<TCollider> staying, IReadOnlyHashSet<TCollider> readOnlyStaying,
                StaticBuffer<TCollider> buffer,
                System.Func<TCollider, bool> isColliderEnabled,
                ReliableCallbacks.Callbacks callbacks)
            {
                var removeCount = 0;
                var removeBuffer = buffer;
                foreach (var stay in staying)
                {
                    // Remove invalid colliders.
                    if (stay == null || isColliderEnabled(stay) == false || stay.gameObject.activeInHierarchy == false)
                    {
                        removeBuffer.AddItemSafely(ref removeCount, stay);
                        continue;
                    }
                    callbacks.OnStay(stay);
                }
                if (removeCount > 0)
                {
                    for (int i = 0; i < removeCount; i++)
                    {
                        var stay = removeBuffer.Buffer[i];
                        staying.Remove(stay);

                        // Always Fire staying changed callback first.
                        callbacks.OnStayingChanged(readOnlyStaying);
                        callbacks.OnExit(stay);
                    }
                }
                removeBuffer.ClearBuffer(removeCount);
            }
        }

        #region Unity Physics Callbacks Handler
        protected void HandleUnityTriggerEnter(TCollider other)
            => this.OnTriggerEnterCallback_Internal(other);
        protected void HandleUnityTriggerExit(TCollider other)
            => this.OnTriggerExitCallback_Internal(other);
        protected void HandleUnityCollisionEnter(TCollision collision)
            => this.OnCollisionEnterCallback_Internal(collision);
        protected void HandleUnityCollisionExit(TCollision collision)
            => this.OnCollisionExitCallback_Internal(collision);
        #endregion Unity Physics Callbacks Handler

        #region Internal Physics Callbacks
        private void OnTriggerEnterCallback_Internal(TCollider other)
        {
            if (this.triggerStayingColliders.Add(other) == false) return;

            // Always Fire staying changed callback first.
            this.reliableCallbacks.Trigger.OnStayingChanged(this.TriggerStayingColliders);
            this.reliableCallbacks.Trigger.OnEnter(other);
        }
        private void OnTriggerExitCallback_Internal(TCollider other)
        {
            if (this.triggerStayingColliders.Remove(other) == false) return;

            // Always Fire staying changed callback first.
            this.reliableCallbacks.Trigger.OnStayingChanged(this.TriggerStayingColliders);
            this.reliableCallbacks.Trigger.OnExit(other);
        }

        private void OnCollisionEnterCallback_Internal(TCollision collision)
        {
            var collider = this.GetCollider(collision);
            if (this.collisionStayingColliders.Add(collider) == false) return;

            // Always Fire staying changed callback first.
            this.reliableCallbacks.Collision.OnStayingChanged(this.CollisionStayingColliders);
            this.reliableCallbacks.Collision.OnEnter(collider);
        }
        private void OnCollisionExitCallback_Internal(TCollision collision)
        {
            var collider = this.GetCollider(collision);
            if (this.collisionStayingColliders.Remove(collider) == false) return;

            // Always Fire staying changed callback first.
            this.reliableCallbacks.Collision.OnStayingChanged(this.CollisionStayingColliders);
            this.reliableCallbacks.Collision.OnExit(collider);
        }
        #endregion Internal Physics Callbacks
    }
}
