using HisaCat.HUE.Collections;
using UnityEngine;

namespace HisaCat.HUE.PhysicsExtension
{
    public abstract class ReliablePhysics2DCallbacks : ReliablePhysicsCallbacksCore<Collider2D, Collision2D>
    {
        protected sealed override ReliableCallbacks DelegateReliableCallbacks()
        {
            return new(
                trigger: new(
                    onEnter: this.OnReliableTrigger2DEnterCallback,
                    onStay: this.OnReliableTrigger2DStayCallback,
                    onExit: this.OnReliableTrigger2DExitCallback,
                    onStayingChanged: this.OnReliableTriggerStayingChanged2DCallback
                ),
                collision: new(
                    onEnter: this.OnReliableCollision2DEnterCallback,
                    onStay: this.OnReliableCollision2DStayCallback,
                    onExit: this.OnReliableCollision2DExitCallback,
                    onStayingChanged: this.OnReliableCollisionStayingChanged2DCallback
                )
            );
        }

        #region Sealed override methods
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
        protected virtual void OnReliableTriggerStayingChanged2DCallback(IReadOnlyHashSet<Collider2D> staying) { }

        protected virtual void OnReliableCollision2DEnterCallback(Collider2D other) { }
        protected virtual void OnReliableCollision2DStayCallback(Collider2D other) { }
        protected virtual void OnReliableCollision2DExitCallback(Collider2D other) { }
        protected virtual void OnReliableCollisionStayingChanged2DCallback(IReadOnlyHashSet<Collider2D> staying) { }
        #endregion Reliable Physics Callbacks
    }
}
