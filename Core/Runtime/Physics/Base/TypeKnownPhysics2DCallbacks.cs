
using HisaCat.HUE.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HisaCat.HUE.PhysicsExtension
{
    public abstract class TypeKnownPhysics2DCallbacks<TTarget> :
        TypeKnownPhysicsCallbacksCore<TTarget, Collider2D, Collision2D>
        where TTarget : Component
    {
        protected override TypeKnownCallbacks DelegateTypeKnownCallbacks()
        {
            return new(
                trigger: new(
                    onEnter: this.OnTargetTriggerEnter2DCallback,
                    onStay: this.OnTargetTriggerStay2DCallback,
                    onExit: this.OnTargetTriggerExit2DCallback,
                    onStayingChanged: this.OnTargetTriggerStayingChanged2DCallback
                ),
                collision: new(
                    onEnter: this.OnTargetCollisionEnter2DCallback,
                    onStay: this.OnTargetCollisionStay2DCallback,
                    onExit: this.OnTargetCollisionExit2DCallback,
                    onStayingChanged: this.OnTargetCollisionStayingChanged2DCallback
                )
            );
        }

        #region Sealed override methods
        protected sealed override bool IsColliderEnabled(Collider2D collider) => collider.enabled;
        protected sealed override Collider2D GetCollider(Collision2D collision) => collision.collider;
        protected sealed override GameObject GetColliderGameObject(Collider2D collider) => collider.gameObject;
        #endregion Sealed override methods

        #region Handle Unity Physics Callbacks
        protected virtual void OnTriggerEnter2D(Collider2D other) => this.HandleUnityTriggerEnter(other);
        protected virtual void OnTriggerExit2D(Collider2D other) => this.HandleUnityTriggerExit(other);
        protected virtual void OnCollisionEnter2D(Collision2D collision) => this.HandleUnityCollisionEnter(collision);
        protected virtual void OnCollisionExit2D(Collision2D collision) => this.HandleUnityCollisionExit(collision);
        #endregion Handle Unity Physics Callbacks


        #region Target Physics Callbacks
        protected virtual void OnTargetTriggerEnter2DCallback(TTarget target) { }
        protected virtual void OnTargetTriggerStay2DCallback(TTarget target) { }
        protected virtual void OnTargetTriggerExit2DCallback(TTarget target) { }
        protected virtual void OnTargetTriggerStayingChanged2DCallback(IReadOnlyDictionary<TTarget, IReadOnlyHashSet<Collider2D>> staying) { }

        protected virtual void OnTargetCollisionStay2DCallback(TTarget target) { }
        protected virtual void OnTargetCollisionEnter2DCallback(TTarget target) { }
        protected virtual void OnTargetCollisionExit2DCallback(TTarget target) { }
        protected virtual void OnTargetCollisionStayingChanged2DCallback(IReadOnlyDictionary<TTarget, IReadOnlyHashSet<Collider2D>> staying) { }
        #endregion Target Physics Callbacks
    }
}
