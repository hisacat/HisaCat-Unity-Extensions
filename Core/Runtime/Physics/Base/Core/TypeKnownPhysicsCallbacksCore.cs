using System.Collections.Generic;
using System.Runtime.CompilerServices;
using HisaCat.HUE.Collections;
using HisaCat.UnityExtensions;
using UnityEngine;

namespace HisaCat.HUE.PhysicsExtension
{
    public abstract class TypeKnownPhysicsCallbacksCore<TTarget, TCollider, TCollision> :
        ReliablePhysicsCallbacksCore<TCollider, TCollision>,
        IReliablePhysicsBridge<TCollider, TCollision>
        where TTarget : Component
        where TCollider : Component where TCollision : class
    {
        #region Abstract Methods
        /// <summary>
        /// Type-Known 충돌 이벤트 처리를 위한 브리지를 반환합니다.
        /// </summary>
        protected abstract ITypeKnownReliablePhysicsBridge<TTarget, TCollider> AsTypeKnownReliablePhysicsBridge();
        /// <summary>
        /// 충돌체에서 TTarget을 찾기 위한 게임 오브젝트를 반환합니다.
        /// </summary>
        /// <param name="collider">충돌체</param>
        /// <returns>게임 오브젝트</returns>
        protected abstract GameObject GetColliderGameObject(TCollider collider);
        #endregion Abstract Methods

        #region Sealed override methods
        protected sealed override IReliablePhysicsBridge<TCollider, TCollision> AsReliablePhysicsBridge() => this;
        #endregion Sealed override methods

        /// <summary>
        /// GetComponent를 최소화하기 위한 각 Collider가 가지고 있는 TTarget의 캐시입니다.
        /// </summary>
        protected abstract Dictionary<TCollider, TTarget> CachedKnownTargetColliders { get; }
        protected abstract StaticBuffer<TTarget> TargetsBuffer { get; }

        protected IReadOnlyDictionary<TTarget, IReadOnlyHashSet<TCollider>> TriggerStayingTargets => this.triggerStayingTargets.ReadOnlyDictionary;
        private ReadOnlyHashSetValueDictionary triggerStayingTargets = null;
        protected IReadOnlyDictionary<TTarget, IReadOnlyHashSet<TCollider>> CollisionStayingTargets => this.collisionStayingTargets.ReadOnlyDictionary;
        private ReadOnlyHashSetValueDictionary collisionStayingTargets = null;
        private class ReadOnlyHashSetValueDictionary : ReadOnlyValueDictionary<TTarget, HashSet<TCollider>, IReadOnlyHashSet<TCollider>>
        {
            public override IReadOnlyHashSet<TCollider> ToReadOnlyValue(HashSet<TCollider> value)
                => value.AsReadOnly();
        }

        public abstract LayerMask TargetLayerMask { get; }

        private ITypeKnownReliablePhysicsBridge<TTarget, TCollider> bridge = null;
        protected override void Awake()
        {
            base.Awake();

            this.bridge = this.AsTypeKnownReliablePhysicsBridge();
        }

        private TTarget FindTarget(TCollider collider)
        {
            if (this.CachedKnownTargetColliders.TryGetValue(collider, out var existingTarget))
            {
                if (IsValidColllider(collider, existingTarget, this.TargetLayerMask, this.GetColliderGameObject))
                    return existingTarget;

                // Remove cached collider if it is not valid anymore.
                this.CachedKnownTargetColliders.Remove(collider);

                static bool IsValidColllider(TCollider collider, TTarget target, LayerMask layerMask, System.Func<TCollider, GameObject> getColliderGameObject)
                {
                    if (target == null) return false;
                    if (layerMask.IsLayerInMask(target.gameObject.layer) == false) return false;
                    var colliderGameObject = getColliderGameObject(collider);
                    if ((colliderGameObject == target.gameObject || colliderGameObject.transform.IsChildOf(target.transform)) == false) return false;
                    return true;
                }
            }

            var colliderGameObject = this.GetColliderGameObject(collider);
            if (this.TargetLayerMask.IsLayerInMask(colliderGameObject.layer) == false) return null;

            var target = colliderGameObject.GetComponentInParent<TTarget>();
            if (target == null) return null;

            this.CachedKnownTargetColliders.Add(collider, target);
            return target;
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();

            OnStayWorks(this.triggerStayingTargets, TargetsBuffer, this.bridge.OnTargetTriggerStayCallback_Internal, this.bridge.OnTargetTriggerExitCallback_Internal, this.bridge.OnTargetTriggerStayingChangedCallback_Internal);
            OnStayWorks(this.collisionStayingTargets, TargetsBuffer, this.bridge.OnTargetCollisionStayCallback_Internal, this.bridge.OnTargetCollisionExitCallback_Internal, this.bridge.OnTargetCollisionStayingChangedCallback_Internal);
            static void OnStayWorks(
                ReadOnlyHashSetValueDictionary staying,
                StaticBuffer<TTarget> targetBuffer,
                System.Action<TTarget> onStayCallback, System.Action<TTarget> onExitCallback,
                System.Action<IReadOnlyDictionary<TTarget, IReadOnlyHashSet<TCollider>>> onStayingChangedCallback)
            {
                var removeCount = 0;
                var removeBuffer = targetBuffer;
                foreach (var stay in staying)
                {
                    (var target, var colliders) = (stay.Key, stay.Value);

                    if (target == null || target.gameObject.activeInHierarchy == false)
                    {
                        removeBuffer.AddItemSafely(ref removeCount, target);
                        continue;
                    }

                    // [NOTE] We don't need check invalid colliders because
                    // ReliablePhysicsCallbacks already call Exit events for invalid colliders.
                    // If all colliders are invalid, the target automatically removed from "OnExitWorks".
                    // {
                    //     colliders.RemoveWhere(IsInvalidCollider);
                    //     static bool IsInvalidCollider(Collider collider)
                    //         => collider == null || collider.enabled == false || collider.gameObject.activeInHierarchy == false;

                    //     if (colliders.Count <= 0)
                    //     {
                    //         removeBuffer.AddItemSafely(ref removeCount, target);
                    //         continue;
                    //     }
                    // }

                    onStayCallback(target);
                }
                for (int i = 0; i < removeCount; i++)
                {
                    var stay = removeBuffer.Buffer[i];
                    staying.Remove(stay);
                    // Always Fire staying changed callback first.
                    onStayingChangedCallback(staying.ReadOnlyDictionary);
                    // And then fire exit callback.
                    onExitCallback(stay);
                }
                removeBuffer.ClearBuffer(removeCount);
            }
        }

        #region IReliablePhysicsBridge
        void IReliablePhysicsBridge<TCollider, TCollision>.OnReliableTriggerEnterCallback_Internal(TCollider other)
            => this.OnReliableTriggerEnterCallback(other);
        void IReliablePhysicsBridge<TCollider, TCollision>.OnReliableTriggerStayCallback_Internal(TCollider other) { }
        void IReliablePhysicsBridge<TCollider, TCollision>.OnReliableTriggerExitCallback_Internal(TCollider other)
            => this.OnReliableTriggerExitCallback(other);
        void IReliablePhysicsBridge<TCollider, TCollision>.OnReliableTriggerStayingChangedCallback_Internal(IReadOnlyHashSet<TCollider> staying) { }

        void IReliablePhysicsBridge<TCollider, TCollision>.OnReliableCollisionEnterCallback_Internal(TCollider other)
            => this.OnReliableCollisionEnterCallback(other);
        void IReliablePhysicsBridge<TCollider, TCollision>.OnReliableCollisionStayCallback_Internal(TCollider other) { }
        void IReliablePhysicsBridge<TCollider, TCollision>.OnReliableCollisionExitCallback_Internal(TCollider other)
            => this.OnReliableCollisionExitCallback(other);
        void IReliablePhysicsBridge<TCollider, TCollision>.OnReliableCollisionStayingChangedCallback_Internal(IReadOnlyHashSet<TCollider> staying) { }
        #endregion IReliablePhysicsBridge

        #region Reliable Physics Callbacks
        protected virtual void OnReliableTriggerEnterCallback(TCollider other)
            => OnEnterWorks(other, this.triggerStayingTargets, this.bridge.OnTargetTriggerEnterCallback_Internal, this.bridge.OnTargetTriggerStayingChangedCallback_Internal);
        protected virtual void OnReliableTriggerExitCallback(TCollider other)
            => OnExitWorks(other, this.triggerStayingTargets, this.bridge.OnTargetTriggerExitCallback_Internal, this.bridge.OnTargetTriggerStayingChangedCallback_Internal);
        protected virtual void OnReliableCollisionEnterCallback(TCollider other)
            => OnEnterWorks(other, this.collisionStayingTargets, this.bridge.OnTargetCollisionEnterCallback_Internal, this.bridge.OnTargetCollisionStayingChangedCallback_Internal);
        protected virtual void OnReliableCollisionExitCallback(TCollider other)
            => OnExitWorks(other, this.collisionStayingTargets, this.bridge.OnTargetCollisionExitCallback_Internal, this.bridge.OnTargetCollisionStayingChangedCallback_Internal);
        #endregion Reliable Physics Callbacks
        private void OnEnterWorks(
            TCollider other, ReadOnlyHashSetValueDictionary stayTargets,
            System.Action<TTarget> onEnterCallback,
            System.Action<IReadOnlyDictionary<TTarget, IReadOnlyHashSet<TCollider>>> onStayingChangedCallback)
        {
            var target = FindTarget(other);
            if (target == null) return;

            if (stayTargets.ContainsKey(target) == false)
            {
                stayTargets.Add(target, new() { other });
                // Always Fire staying changed callback first.
                onStayingChangedCallback(stayTargets.ReadOnlyDictionary);
                // And then fire enter callback.
                onEnterCallback(target);
            }
            else
            {
                stayTargets[target].Add(other);
            }
        }
        private void OnExitWorks(
            TCollider other, ReadOnlyHashSetValueDictionary stayTargets,
            System.Action<TTarget> onEnterCallback,
            System.Action<IReadOnlyDictionary<TTarget, IReadOnlyHashSet<TCollider>>> onStayingChangedCallback)
        {
            var target = FindTarget(other);
            if (target == null) return;
            if (stayTargets.ContainsKey(target) == false) return;

            stayTargets[target].Remove(other);
            if (stayTargets[target].Count <= 0)
            {
                stayTargets.Remove(target);
                // Always Fire staying changed callback first.
                onStayingChangedCallback(stayTargets.ReadOnlyDictionary);
                // And then fire exit callback.
                onEnterCallback(target);
            }
        }

        // #region Target Physics Callbacks
        // protected virtual void OnTargetTriggerEnterCallback(TTarget target) { }
        // protected virtual void OnTargetTriggerStayCallback(TTarget target) { }
        // protected virtual void OnTargetTriggerExitCallback(TTarget target) { }
        // protected virtual void OnTargetTriggerStayingChangedCallback(IReadOnlyDictionary<TTarget, IReadOnlyHashSet<Collider>> staying) { }

        // protected virtual void OnTargetCollisionStayCallback(TTarget target) { }
        // protected virtual void OnTargetCollisionEnterCallback(TTarget target) { }
        // protected virtual void OnTargetCollisionExitCallback(TTarget target) { }
        // protected virtual void OnTargetCollisionStayingChangedCallback(IReadOnlyDictionary<TTarget, IReadOnlyHashSet<Collider>> staying) { }
        // #endregion Target Physics Callbacks

        #region Debugs
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void LogError(string message)
            => Debug.LogError($"[{this.GetType().Name}] {message}");
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void LogError(string message, UnityEngine.Object context)
            => Debug.LogError($"[{this.GetType().Name}] {message}", context);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void LogWarning(string message)
            => Debug.LogWarning($"[{this.GetType().Name}] {message}");
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void LogWarning(string message, UnityEngine.Object context)
            => Debug.LogWarning($"[{this.GetType().Name}] {message}", context);
        #endregion
    }
    public interface ITypeKnownReliablePhysicsBridge<TTarget, TCollider>
    {
        void OnTargetTriggerEnterCallback_Internal(TTarget other);
        void OnTargetTriggerStayCallback_Internal(TTarget other);
        void OnTargetTriggerExitCallback_Internal(TTarget other);
        void OnTargetTriggerStayingChangedCallback_Internal(IReadOnlyDictionary<TTarget, IReadOnlyHashSet<TCollider>> staying);

        void OnTargetCollisionEnterCallback_Internal(TTarget other);
        void OnTargetCollisionStayCallback_Internal(TTarget other);
        void OnTargetCollisionExitCallback_Internal(TTarget other);
        void OnTargetCollisionStayingChangedCallback_Internal(IReadOnlyDictionary<TTarget, IReadOnlyHashSet<TCollider>> staying);
    }
}
