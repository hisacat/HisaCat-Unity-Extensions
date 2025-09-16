using System.Collections.Generic;
using System.Runtime.CompilerServices;
using HisaCat.HUE.Collections;
using HisaCat.UnityExtensions;
using UnityEngine;

namespace HisaCat.HUE.PhysicsExtension
{
    [RequireComponent(typeof(Collider))]
    public abstract class TypeKnownPhysicsCallbacks<TTarget> :
        ReliablePhysicsCallbacksCore<Collider, Collision>,
        IReliablePhysicsBridge<Collider, Collision>
        where TTarget : Component
    {
        /// <summary>
        /// GetComponent를 최소화하기 위한 각 Collider가 가지고 있는 TTarget의 캐시입니다.
        /// </summary>
        protected abstract Dictionary<Collider, TTarget> CachedKnownTargetColliders { get; }
        protected abstract StaticBuffer<TTarget> TargetsBuffer { get; }

        protected IReadOnlyDictionary<TTarget, IReadOnlyHashSet<Collider>> TriggerStayingTargets => this.triggerStayingTargets.ReadOnlyDictionary;
        private ReadOnlyHashSetValueDictionary triggerStayingTargets = null;
        protected IReadOnlyDictionary<TTarget, IReadOnlyHashSet<Collider>> CollisionStayingTargets => this.collisionStayingTargets.ReadOnlyDictionary;
        private ReadOnlyHashSetValueDictionary collisionStayingTargets = null;
        private class ReadOnlyHashSetValueDictionary : ReadOnlyValueDictionary<TTarget, HashSet<Collider>, IReadOnlyHashSet<Collider>>
        {
            public override IReadOnlyHashSet<Collider> ToReadOnlyValue(HashSet<Collider> value)
                => value.AsReadOnly();
        }

        public abstract LayerMask TargetLayerMask { get; }


        private TTarget FindTarget(Collider collider)
        {
            if (this.CachedKnownTargetColliders.TryGetValue(collider, out var existingTarget))
            {
                if (IsValidColllider(collider, existingTarget, this.TargetLayerMask))
                    return existingTarget;

                // Remove cached collider if it is not valid anymore.
                this.CachedKnownTargetColliders.Remove(collider);

                static bool IsValidColllider(Collider collider, TTarget target, LayerMask layerMask)
                {
                    if (target == null) return false;
                    if (layerMask.IsLayerInMask(target.gameObject.layer) == false) return false;
                    if ((collider.gameObject == target.gameObject || collider.transform.IsChildOf(target.transform)) == false) return false;
                    return true;
                }
            }

            if (this.TargetLayerMask.IsLayerInMask(collider.gameObject.layer) == false) return null;
            var target = collider.GetComponentInParent<TTarget>();
            if (target == null) return null;
            this.CachedKnownTargetColliders.Add(collider, target);
            return target;
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();

            OnStayWorks(this.triggerStayingTargets, TargetsBuffer, this.OnTargetTriggerStayCallback, this.OnTargetTriggerExitCallback, this.OnTargetTriggerStayingChangedCallback);
            OnStayWorks(this.collisionStayingTargets, TargetsBuffer, this.OnTargetCollisionStayCallback, this.OnTargetCollisionExitCallback, this.OnTargetCollisionStayingChangedCallback);
            static void OnStayWorks(
                ReadOnlyHashSetValueDictionary staying,
                StaticBuffer<TTarget> targetBuffer,
                System.Action<TTarget> onStayCallback, System.Action<TTarget> onExitCallback,
                System.Action<IReadOnlyDictionary<TTarget, IReadOnlyHashSet<Collider>>> onStayingChangedCallback)
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
        public void OnReliableTriggerEnterCallback_Internal(Collider other)
            => this.OnReliableTriggerEnterCallback(other);
        public void OnReliableTriggerStayCallback_Internal(Collider other) { }
        public void OnReliableTriggerExitCallback_Internal(Collider other)
            => this.OnReliableTriggerExitCallback(other);
        public void OnReliableTriggerStayingChangedCallback_Internal(IReadOnlyHashSet<Collider> staying) { }

        public void OnReliableCollisionEnterCallback_Internal(Collider other)
            => this.OnReliableCollisionEnterCallback(other);
        public void OnReliableCollisionStayCallback_Internal(Collider other) { }
        public void OnReliableCollisionExitCallback_Internal(Collider other)
            => this.OnReliableCollisionExitCallback(other);
        public void OnReliableCollisionStayingChangedCallback_Internal(IReadOnlyHashSet<Collider> staying) { }
        #endregion IReliablePhysicsBridge

        #region Reliable Physics Callbacks
        protected virtual void OnReliableTriggerEnterCallback(Collider other)
            => OnEnterWorks(other, this.triggerStayingTargets, this.OnTargetTriggerEnterCallback, this.OnTargetTriggerStayingChangedCallback);
        protected virtual void OnReliableTriggerExitCallback(Collider other)
            => OnExitWorks(other, this.triggerStayingTargets, this.OnTargetTriggerExitCallback, this.OnTargetTriggerStayingChangedCallback);
        protected virtual void OnReliableCollisionEnterCallback(Collider other)
            => OnEnterWorks(other, this.collisionStayingTargets, this.OnTargetCollisionEnterCallback, this.OnTargetCollisionStayingChangedCallback);
        protected virtual void OnReliableCollisionExitCallback(Collider other)
            => OnExitWorks(other, this.collisionStayingTargets, this.OnTargetCollisionExitCallback, this.OnTargetCollisionStayingChangedCallback);
        #endregion Reliable Physics Callbacks
        private void OnEnterWorks(
            Collider other, ReadOnlyHashSetValueDictionary stayTargets,
            System.Action<TTarget> onEnterCallback,
            System.Action<IReadOnlyDictionary<TTarget, IReadOnlyHashSet<Collider>>> onStayingChangedCallback)
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
            Collider other, ReadOnlyHashSetValueDictionary stayTargets,
            System.Action<TTarget> onEnterCallback,
            System.Action<IReadOnlyDictionary<TTarget, IReadOnlyHashSet<Collider>>> onStayingChangedCallback)
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
}
