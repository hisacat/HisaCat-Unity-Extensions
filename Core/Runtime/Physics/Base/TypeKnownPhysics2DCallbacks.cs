using HisaCat.HUE.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HisaCat.HUE.PhysicsExtension
{
    public abstract class TypeKnownPhysics2DCallbacks<TTarget> :
        TypeKnownPhysicsCallbacksCore<TTarget, Collider2D, Collision2D>,
        IReliablePhysicsBridge<Collider2D, Collision2D>,
        ITypeKnownReliablePhysicsBridge<TTarget, Collider2D>
        where TTarget : Component
    {
        #region Sealed override methods
        protected sealed override ITypeKnownReliablePhysicsBridge<TTarget, Collider2D> AsTypeKnownReliablePhysicsBridge() => this;
        #endregion Sealed override methods

        public void OnReliableCollisionEnterCallback_Internal(Collider2D other)
        {
            throw new System.NotImplementedException();
        }

        public void OnReliableCollisionExitCallback_Internal(Collider2D other)
        {
            throw new System.NotImplementedException();
        }

        public void OnReliableCollisionStayCallback_Internal(Collider2D other)
        {
            throw new System.NotImplementedException();
        }

        public void OnReliableCollisionStayingChangedCallback_Internal(IReadOnlyHashSet<Collider2D> staying)
        {
            throw new System.NotImplementedException();
        }

        public void OnReliableTriggerEnterCallback_Internal(Collider2D other)
        {
            throw new System.NotImplementedException();
        }

        public void OnReliableTriggerExitCallback_Internal(Collider2D other)
        {
            throw new System.NotImplementedException();
        }

        public void OnReliableTriggerStayCallback_Internal(Collider2D other)
        {
            throw new System.NotImplementedException();
        }

        public void OnReliableTriggerStayingChangedCallback_Internal(IReadOnlyHashSet<Collider2D> staying)
        {
            throw new System.NotImplementedException();
        }

        public void OnTargetTriggerEnterCallback_Internal(TTarget other)
        {
            throw new System.NotImplementedException();
        }

        public void OnTargetTriggerStayCallback_Internal(TTarget other)
        {
            throw new System.NotImplementedException();
        }

        public void OnTargetTriggerExitCallback_Internal(TTarget other)
        {
            throw new System.NotImplementedException();
        }

        public void OnTargetTriggerStayingChangedCallback_Internal(IReadOnlyDictionary<TTarget, IReadOnlyHashSet<Collider2D>> staying)
        {
            throw new System.NotImplementedException();
        }

        public void OnTargetCollisionEnterCallback_Internal(TTarget other)
        {
            throw new System.NotImplementedException();
        }

        public void OnTargetCollisionStayCallback_Internal(TTarget other)
        {
            throw new System.NotImplementedException();
        }

        public void OnTargetCollisionExitCallback_Internal(TTarget other)
        {
            throw new System.NotImplementedException();
        }

        public void OnTargetCollisionStayingChangedCallback_Internal(IReadOnlyDictionary<TTarget, IReadOnlyHashSet<Collider2D>> staying)
        {
            throw new System.NotImplementedException();
        }
    }
}