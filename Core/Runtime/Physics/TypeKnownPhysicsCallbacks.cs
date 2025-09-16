using System.Collections.Generic;
using System.Runtime.CompilerServices;
using HisaCat.HUE.Collections;
using HisaCat.UnityExtensions;
using UnityEngine;

namespace HisaCat.HUE.PhysicsExtension
{
    [RequireComponent(typeof(Collider))]
    public abstract class TypeKnownPhysicsCallbacks<TTarget> : ReliablePhysicsCallbacks where TTarget : Component
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

            OnStayWorks(this.triggerStayingTargets, TargetsBuffer, this.OnTargetTriggerStay, this.OnTargetTriggerExit);
            OnStayWorks(this.collisionStayingTargets, TargetsBuffer, this.OnTargetCollisionStay, this.OnTargetCollisionExit);
            static void OnStayWorks(ReadOnlyHashSetValueDictionary staying, StaticBuffer<TTarget> targetBuffer, System.Action<TTarget> onStayCallback, System.Action<TTarget> onExitCallback)
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
                    {
                        colliders.RemoveWhere(IsInvalidCollider);
                        static bool IsInvalidCollider(Collider collider)
                            => collider == null || collider.enabled == false || collider.gameObject.activeInHierarchy == false;

                        if (colliders.Count <= 0)
                        {
                            removeBuffer.AddItemSafely(ref removeCount, target);
                            continue;
                        }
                    }

                    onStayCallback(target);
                }
                for (int i = 0; i < removeCount; i++)
                {
                    var stay = removeBuffer.Buffer[i];
                    staying.Remove(stay);
                    onExitCallback(stay);
                }
                removeBuffer.ClearBuffer(removeCount);
            }
        }

        #region Reliable Physics Callbacks
        protected override void OnReliableTriggerEnterCallback(Collider other)
            => OnEnterWorks(other, this.triggerStayingTargets, this.OnTargetTriggerEnter);
        protected override void OnReliableTriggerExitCallback(Collider other)
            => OnExitWorks(other, this.triggerStayingTargets, this.OnTargetTriggerExit);
        protected override void OnReliableCollisionEnterCallback(Collider other)
            => OnEnterWorks(other, this.collisionStayingTargets, this.OnTargetCollisionEnter);
        protected override void OnReliableCollisionExitCallback(Collider other)
            => OnExitWorks(other, this.collisionStayingTargets, this.OnTargetCollisionExit);
        private void OnEnterWorks(Collider other, ReadOnlyHashSetValueDictionary stayTargets, System.Action<TTarget> onEnterCallback)
        {
            var target = FindTarget(other);
            if (target == null) return;

            if (stayTargets.ContainsKey(target) == false)
            {
                stayTargets.Add(target, new() { other });
                onEnterCallback(target);
            }
            else
            {
                stayTargets[target].Add(other);
            }
        }
        private void OnExitWorks(Collider other, ReadOnlyHashSetValueDictionary stayTargets, System.Action<TTarget> onEnterCallback)
        {
            var target = FindTarget(other);
            if (target == null) return;
            if (stayTargets.ContainsKey(target) == false) return;

            stayTargets[target].Remove(other);
            if (stayTargets[target].Count == 0)
            {
                stayTargets.Remove(target);
                onEnterCallback(target);
            }
        }
        #endregion Reliable Physics Callbacks

        #region Target Physics Callbacks
        protected virtual void OnTargetTriggerEnter(TTarget target) { }
        protected virtual void OnTargetTriggerStay(TTarget target) { }
        protected virtual void OnTargetTriggerExit(TTarget target) { }
        protected virtual void OnTargetCollisionStay(TTarget target) { }
        protected virtual void OnTargetCollisionEnter(TTarget target) { }
        protected virtual void OnTargetCollisionExit(TTarget target) { }
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
