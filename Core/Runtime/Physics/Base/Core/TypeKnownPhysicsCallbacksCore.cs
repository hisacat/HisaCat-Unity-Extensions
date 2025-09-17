using System.Collections.Generic;
using System.Runtime.CompilerServices;
using HisaCat.HUE.Collections;
using HisaCat.UnityExtensions;
using UnityEngine;

namespace HisaCat.HUE.PhysicsExtension
{
    /// <summary>
    /// Provides reliable, type-aware physics callbacks for the target component type <typeparamref name="TTarget"/>.
    /// Ensures that physics enter/stay/exit events<br/>
    /// are invoked consistently, even when Unity would normally skip them.<br/>
    /// It also manages the collection of currently colliding objects.
    /// <para>
    /// Guaranteed callback invocation order:<br/>
    /// Always StayingChanged callback fire first.
    /// - On Enter: StayingChanged → Enter<br/>
    /// - On Exit: StayingChanged → Exit
    /// </para>
    /// </summary>
    [DefaultExecutionOrder(int.MinValue)]
    public abstract class TypeKnownPhysicsCallbacksCore<TTarget, TCollider, TCollision> :
        ReliablePhysicsCallbacksCore<TCollider, TCollision>
        where TTarget : Component
        where TCollider : Component where TCollision : class
    {
        #region Abstract Methods
        /// <summary>
        /// Gets the target layer mask. Use <see cref="Physics.AllLayers"/> to match all layers.
        /// </summary>
        public abstract LayerMask TargetLayerMask { get; }
        /// <summary>
        /// Provides the delegated callbacks used by Type-Known Physics Callbacks.
        /// </summary>
        protected abstract TypeKnownCallbacks DelegateTypeKnownCallbacks();
        /// <summary>
        /// Extracts the gameobject from the given collider.
        /// </summary>
        protected abstract GameObject GetColliderGameObject(TCollider collider);
        #endregion Abstract Methods

        #region Caches
        /// <summary>
        /// Targets buffer for optimization.
        /// </summary>
        private readonly StaticBuffer<TTarget> targetsBuffer
            = PhysicsCallbackCache.GetStaticBuffer<TTarget>(1024);
        /// <summary>
        /// Collider's target cache for optimization.
        /// This caches the component retrieved from each collider to minimize GetComponent calls.
        /// </summary>
        private readonly Dictionary<TCollider, TTarget> colliderTargetCache
            = PhysicsCallbackCache.GetStaticDictionary<TCollider, TTarget>(1024);
        #endregion Caches

        protected sealed override ReliableCallbacks DelegateReliableCallbacks()
        {
            return new(
                trigger: new(
                    onEnter: this.OnReliableTriggerEnterCallback,
                    onStay: this.OnReliableTriggerStayCallback,
                    onExit: this.OnReliableTriggerExitCallback,
                    onStayingChanged: this.OnReliableTriggerStayingChangedCallback
                ),
                collision: new(
                    onEnter: this.OnReliableCollisionEnterCallback,
                    onStay: this.OnReliableCollisionStayCallback,
                    onExit: this.OnReliableCollisionExitCallback,
                    onStayingChanged: this.OnReliableCollisionStayingChangedCallback
                )
            );
        }

        protected IReadOnlyDictionary<TTarget, IReadOnlyHashSet<TCollider>> TriggerStayingTargets => this.triggerStayingTargets.ReadOnlyDictionary;
        private ReadOnlyHashSetValueDictionary triggerStayingTargets = null;
        protected IReadOnlyDictionary<TTarget, IReadOnlyHashSet<TCollider>> CollisionStayingTargets => this.collisionStayingTargets.ReadOnlyDictionary;
        private ReadOnlyHashSetValueDictionary collisionStayingTargets = null;
        private class ReadOnlyHashSetValueDictionary : ReadOnlyValueDictionary<TTarget, HashSet<TCollider>, IReadOnlyHashSet<TCollider>>
        {
            public override IReadOnlyHashSet<TCollider> ToReadOnlyValue(HashSet<TCollider> value)
                => value.AsReadOnly();
        }

        private TypeKnownCallbacks typeKnownCallbacks = null;
        protected override void Awake()
        {
            base.Awake();

            this.triggerStayingTargets = new();
            this.collisionStayingTargets = new();

            this.typeKnownCallbacks = this.DelegateTypeKnownCallbacks();
        }

        public class TypeKnownCallbacks
        {
            public delegate void TargetDelegate(TTarget other);
            public delegate void ReadOnlyTargetsDelegate(IReadOnlyDictionary<TTarget, IReadOnlyHashSet<TCollider>> staying);

            public readonly Callbacks Trigger;
            public readonly Callbacks Collision;
            public class Callbacks
            {
                public readonly TargetDelegate OnEnter;
                public readonly TargetDelegate OnStay;
                public readonly TargetDelegate OnExit;
                public readonly ReadOnlyTargetsDelegate OnStayingChanged;

                public Callbacks(
                    TargetDelegate onEnter, TargetDelegate onStay, TargetDelegate onExit,
                    ReadOnlyTargetsDelegate onStayingChanged)
                {
                    this.OnEnter = onEnter;
                    this.OnStay = onStay;
                    this.OnExit = onExit;
                    this.OnStayingChanged = onStayingChanged;
                }
            }

            public TypeKnownCallbacks(Callbacks trigger, Callbacks collision)
            {
                this.Trigger = trigger;
                this.Collision = collision;
            }
        }

        private TTarget FindTarget(TCollider collider)
        {
            if (this.colliderTargetCache.TryGetValue(collider, out var existingTarget))
            {
                if (IsValidCollider(collider, existingTarget, this.TargetLayerMask, this.GetColliderGameObject))
                    return existingTarget;

                static bool IsValidCollider(TCollider collider, TTarget target, LayerMask layerMask, System.Func<TCollider, GameObject> getColliderGameObject)
                {
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

            this.colliderTargetCache.Add(collider, target);
            return target;
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();

            OnStayWorks(this.triggerStayingTargets, targetsBuffer, this.IsColliderEnabled, this.typeKnownCallbacks.Trigger);
            OnStayWorks(this.collisionStayingTargets, targetsBuffer, this.IsColliderEnabled, this.typeKnownCallbacks.Collision);
            static void OnStayWorks(
                ReadOnlyHashSetValueDictionary staying,
                StaticBuffer<TTarget> targetBuffer,
                System.Func<TCollider, bool> isColliderEnabled,
                TypeKnownCallbacks.Callbacks callbacks)
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
                    //     bool IsInvalidCollider(TCollider collider)
                    //         => collider == null || isColliderEnabled(collider) == false || collider.gameObject.activeInHierarchy == false;

                    //     if (colliders.Count <= 0)
                    //     {
                    //         removeBuffer.AddItemSafely(ref removeCount, target);
                    //         continue;
                    //     }
                    // }

                    callbacks.OnStay(target);
                }
                for (int i = 0; i < removeCount; i++)
                {
                    var stay = removeBuffer.Buffer[i];
                    staying.Remove(stay);

                    // Always Fire staying changed callback first.
                    callbacks.OnStayingChanged(staying.ReadOnlyDictionary);
                    callbacks.OnExit(stay);
                }
                removeBuffer.ClearBuffer(removeCount);
            }
        }

        #region Reliable Physics Callbacks
        private void OnReliableTriggerEnterCallback(TCollider other)
            => OnEnterWorks(other, this.triggerStayingTargets, this.typeKnownCallbacks.Trigger);
        private void OnReliableTriggerStayCallback(TCollider other) { }
        private void OnReliableTriggerExitCallback(TCollider other)
            => OnExitWorks(other, this.triggerStayingTargets, this.typeKnownCallbacks.Trigger);
        private void OnReliableTriggerStayingChangedCallback(IReadOnlyHashSet<TCollider> staying) { }

        private void OnReliableCollisionEnterCallback(TCollider other)
            => OnEnterWorks(other, this.collisionStayingTargets, this.typeKnownCallbacks.Collision);
        private void OnReliableCollisionStayCallback(TCollider other) { }
        private void OnReliableCollisionExitCallback(TCollider other)
            => OnExitWorks(other, this.collisionStayingTargets, this.typeKnownCallbacks.Collision);
        private void OnReliableCollisionStayingChangedCallback(IReadOnlyHashSet<TCollider> staying) { }
        #endregion Reliable Physics Callbacks

        private void OnEnterWorks(
            TCollider other, ReadOnlyHashSetValueDictionary stayTargets,
            TypeKnownCallbacks.Callbacks callbacks)
        {
            var target = FindTarget(other);
            if (target == null) return;

            if (stayTargets.ContainsKey(target) == false)
            {
                stayTargets.Add(target, new() { other });

                // Always Fire staying changed callback first.
                callbacks.OnStayingChanged(stayTargets.ReadOnlyDictionary);
                callbacks.OnEnter(target);
            }
            else
            {
                // Target is already in the staying dictionary (entered by another collider).
                // Don't fire Enter callback again or modify the staying dictionary.
                // Just add this additional collider to the target's colliders HashSet.
                stayTargets[target].Add(other);
            }
        }
        private void OnExitWorks(
            TCollider other, ReadOnlyHashSetValueDictionary stayTargets,
            TypeKnownCallbacks.Callbacks callbacks)
        {
            // Handle callbacks from destroyed collider.
            TTarget target;
            if (other == null)
            {
                target = FindTargetWithDestroyedCollider(stayTargets, other, this.colliderTargetCache);
                static TTarget FindTargetWithDestroyedCollider(
                    ReadOnlyHashSetValueDictionary stayTargets, TCollider colliderTargetCache,
                    Dictionary<TCollider, TTarget> cache)
                {
                    if (cache.ContainsKey(colliderTargetCache))
                    {
                        var target = cache[colliderTargetCache];

                        // Remove destroyed collider from cache.
                        cache.Remove(colliderTargetCache);
                        return target;
                    }
                    else
                    {
                        ManagedDebug.LogWarning(
                            $"[{nameof(TypeKnownPhysicsCallbacksCore<TTarget, TCollider, TCollision>)}] "
                            + "\r\nFailed to find destroyed collider from caching. "
                            + "Trying to find from staying dictionary values. "
                            + "This may cause performance issue.");

                        using (var enumerator = stayTargets.GetEnumerator())
                        {
                            while (enumerator.MoveNext())
                            {
                                var colliders = enumerator.Current.Value;
                                if (colliders.Contains(colliderTargetCache))
                                    return enumerator.Current.Key;
                            }
                        }
                    }
                    return null;
                }
            }
            else
            {
                target = FindTarget(other);
                if (target.IsActuallyNull()) return;
            }

            if (stayTargets.ContainsKey(target) == false) return;

            stayTargets[target].Remove(other);
            if (stayTargets[target].Count <= 0)
            {
                stayTargets.Remove(target);

                // Always Fire staying changed callback first.
                callbacks.OnStayingChanged(stayTargets.ReadOnlyDictionary);
                callbacks.OnExit(target);
            }

        }

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
