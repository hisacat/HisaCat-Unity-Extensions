using HisaCat.UnityExtensions;
using System.Collections.Generic;
using UnityEngine;

namespace HisaCat.PhysicsExtension
{
    public class TypeKnownPhysics2DCallback<TType> : MonoBehaviour where TType : Component
    {
        private List<TType> targets = null;
        private List<TType> targetsInTrigger = null;
        private List<TType> targetsInCollision = null;

        protected virtual void Awake()
        {
            this.targets = new();
            this.targetsInTrigger = new();
            this.targetsInCollision = new();

            this.cachedColliderTargets = new();
            this.cachedCollisionTargets = new();

            InitializeValidateAndInvokeStayCallbacksWorks();
        }
        private void OnDestroy()
        {
            this.targetsInTrigger.Clear(); this.targetsInTrigger = null;
            this.targetsInCollision.Clear(); this.targetsInCollision = null;
        }

        public delegate void PhysicsEventHandler(TType target);

        public event PhysicsEventHandler OnTargetEnter = null;
        public event PhysicsEventHandler OnTargetExit = null;
        public event PhysicsEventHandler OnTargetStay = null;

        public event PhysicsEventHandler OnTargetTriggerEnter = null;
        public event PhysicsEventHandler OnTargetTriggerExit = null;
        public event PhysicsEventHandler OnTargetTriggerStay = null;
        public event PhysicsEventHandler OnTargetCollisionEnter = null;
        public event PhysicsEventHandler OnTargetCollisionExit = null;
        public event PhysicsEventHandler OnTargetCollisionStay = null;

        public IReadOnlyList<TType> Targets => this.targets;
        public IReadOnlyList<TType> TargetsInTrigger => this.targetsInTrigger;
        public IReadOnlyList<TType> TargetsInCollision => this.targetsInCollision;

        #region Physics2D Callbacks
        private Dictionary<Collider2D, TType> cachedColliderTargets = null;
        private Dictionary<Collider2D, TType> cachedCollisionTargets = null;
        protected void OnTriggerEnter2D(Collider2D collision)
            => ParsePhysics2DCallbacks(collision, true, this.targetsInTrigger, OnTargetTriggerEnterCallback, OnTargetEnterCallback, this.OnTargetTriggerEnter, this.OnTargetEnter);
        protected void OnTriggerExit2D(Collider2D collision)
            => ParsePhysics2DCallbacks(collision, false, this.targetsInTrigger, OnTargetTriggerExitCallback, OnTargetExitCallback, this.OnTargetTriggerExit, this.OnTargetExit);
        protected void OnCollisionEnter2D(Collision2D collision)
            => ParsePhysics2DCallbacks(collision, true, this.targetsInTrigger, OnTargetCollisionEnterCallback, OnTargetEnterCallback, this.OnTargetCollisionEnter, this.OnTargetEnter);
        protected void OnCollisionExit2D(Collision2D collision)
            => ParsePhysics2DCallbacks(collision, false, this.targetsInTrigger, OnTargetCollisionExitCallback, OnTargetExitCallback, this.OnTargetCollisionExit, this.OnTargetExit);

        private bool TryGetTargetFromPhysicsCallback(Dictionary<Collider2D, TType> cache, Collider2D collision, bool isEnter, out TType target)
        {
            if (isEnter)
            {
                if(collision.TryGetComponent(out target))
                {
                    cache.TryAdd(collision, target);
                    return true;
                }
            }
            else if (cache.TryGetValue(collision, out target))
            {
                cache.Remove(collision);
                return target != null;
            }
            return false;
        }
        private bool TryGetTargetFromPhysics2DTriggerCallback(Collider2D collision, bool isEnter, out TType target) => TryGetTargetFromPhysicsCallback(this.cachedColliderTargets, collision, isEnter, out target);
        private bool TryGetTargetFromPhysics2DCollisionCallback(Collision2D collision, bool isEnter, out TType target) => TryGetTargetFromPhysicsCallback(this.cachedCollisionTargets, collision.collider, isEnter, out target);

        private void ParsePhysics2DCallbacks(Collider2D collision, bool isEnter, List<TType> container, PhysicsEventHandler callback0, PhysicsEventHandler callback1, PhysicsEventHandler event0, PhysicsEventHandler event1)
        {
            if (TryGetTargetFromPhysics2DTriggerCallback(collision, isEnter, out var target)) ParsePhysics2DCallbacks(target, isEnter, container, callback0, callback1, event0, event1);
        }
        private void ParsePhysics2DCallbacks(Collision2D collision, bool isEnter, List<TType> container, PhysicsEventHandler callback0, PhysicsEventHandler callback1, PhysicsEventHandler event0, PhysicsEventHandler event1)
        {
            if (TryGetTargetFromPhysics2DCollisionCallback(collision, isEnter, out var target)) ParsePhysics2DCallbacks(target, isEnter, container, callback0, callback1, event0, event1);
        }
        private void ParsePhysics2DCallbacks(TType target, bool isEnter, List<TType> container, PhysicsEventHandler callback0, PhysicsEventHandler callback1, PhysicsEventHandler event0, PhysicsEventHandler event1)
        {
            if (isEnter)
            {
                if (this.targets.Contains(target) == false) this.targets.Add(target);
                container.Add(target);
            }
            else
            {
                this.targets.Remove(target);
                container.Remove(target);
            }
            callback0.Invoke(target); callback1.Invoke(target);
            event0?.Invoke(target); event1?.Invoke(target);
        }

        // For avoid GC.Alloc(), use lambda work cache.
        private System.Action<TType> validateAndInvokeStayCallbacks_TriggerWork = null;
        private System.Action<TType> validateAndInvokeStayCallbacks_CollisionWork = null;
        private static System.Action<TType> CreateValidateAndInvokeStayCallbacks(List<TType> container, PhysicsEventHandler callback0, PhysicsEventHandler callback1, PhysicsEventHandler event0, PhysicsEventHandler event1)
        {
            return new System.Action<TType>((target) =>
            {
                if (target == null)
                {
                    container.Remove(target);
                    return;
                }
                callback0.Invoke(target); event0?.Invoke(target);
                callback1.Invoke(target); event1?.Invoke(target);
            });
        }
        private void InitializeValidateAndInvokeStayCallbacksWorks()
        {
            this.validateAndInvokeStayCallbacks_TriggerWork = CreateValidateAndInvokeStayCallbacks(this.targetsInTrigger, OnTargetTriggerStayCallback, OnTargetStayCallback, this.OnTargetTriggerStay, this.OnTargetStay);
            this.validateAndInvokeStayCallbacks_CollisionWork = CreateValidateAndInvokeStayCallbacks(this.targetsInCollision, OnTargetCollisionStayCallback, OnTargetStayCallback, this.OnTargetCollisionStay, this.OnTargetStay);
        }
        protected void FixedUpdate()
        {
            this.targetsInTrigger.ForEachFromEndFast(validateAndInvokeStayCallbacks_TriggerWork);
            this.targetsInCollision.ForEachFromEndFast(validateAndInvokeStayCallbacks_CollisionWork);
        }
        #endregion Physics2D Callbacks

        protected virtual void OnTargetEnterCallback(TType target) { }
        protected virtual void OnTargetExitCallback(TType target) { }
        protected virtual void OnTargetStayCallback(TType target) { }

        protected virtual void OnTargetTriggerEnterCallback(TType target) { }
        protected virtual void OnTargetTriggerExitCallback(TType target) { }
        protected virtual void OnTargetTriggerStayCallback(TType target) { }
        protected virtual void OnTargetCollisionEnterCallback(TType target) { }
        protected virtual void OnTargetCollisionExitCallback(TType target) { }
        protected virtual void OnTargetCollisionStayCallback(TType target) { }
    }
}
