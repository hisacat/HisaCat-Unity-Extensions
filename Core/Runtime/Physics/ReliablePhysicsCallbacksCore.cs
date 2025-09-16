using HisaCat.HUE.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HisaCat.HUE.PhysicsExtension
{
    /// <summary>
    /// ReliablePhysicsCallbacks ensures that physics enter/stay/exit events<br/>
    /// are invoked consistently, even when Unity would normally skip them.
    /// <para>
    /// Unity 기본 동작:
    /// - 자기 자신(GameObject) 또는 충돌 대상이 <b>Disable</b>되거나 <b>Destroy</b>될 때
    ///   OnTriggerExit / OnCollisionExit 이벤트는 호출되지 않습니다.
    /// </para>
    /// <para>
    /// 본 클래스는 해당 누락을 보완하여,<br/>
    /// - Disable/Destroy 시에도 Exit 콜백을 호출하고<br/>
    /// - Staying 집합을 자체 관리하여 일관성 있는 콜백 시퀀스를 제공합니다.<br/>
    /// </para>
    /// <para>
    /// 이벤트 호출 순서<br/>
    /// - On Enter: Enter > StayingChanged<br/>
    /// - On Exit: StayingChanged > Exit
    /// </para>
    /// </summary>
    public abstract class ReliablePhysicsCallbacksCore<TCollider, TCollision> : MonoBehaviour
        where TCollider : Component where TCollision : class
    {
        #region Abstract Methods
        /// <summary>
        /// 충돌체가 활성화되어 있는지 확인합니다.
        /// </summary>
        /// <param name="collider">충돌체</param>
        /// <returns>활성화되어 있으면 true, 그렇지 않으면 false</returns>
        protected abstract bool IsColliderEnabled(TCollider collider);
        /// <summary>
        /// 충돌 정보에서 충돌체를 가져옵니다.
        /// </summary>
        /// <param name="collision">충돌 정보</param>
        /// <returns>충돌체</returns>
        protected abstract TCollider GetCollider(TCollision collision);
        /// <summary>
        /// 정적 충돌체 버퍼.<br/>
        /// Generic에서는 InitializeOnEnterPlayMode attribute를 사용할 수 없음으로,<br/>
        /// Domain reload를 지원하기 위해서는 상속된 클래스에서 정의되어야 합니다.
        /// </summary>
        protected abstract StaticBuffer<TCollider> ColliderBuffer { get; }
        #endregion Abstract Methods

        public IReadOnlyHashSet<TCollider> TriggerStayingColliders { get; private set; } = null;
        private HashSet<TCollider> triggerStayingColliders = null;
        public IReadOnlyHashSet<TCollider> CollisionStayingColliders { get; private set; } = null;
        private HashSet<TCollider> collisionStayingColliders = null;

        private IReliablePhysicsBridge<TCollider, TCollision> core = null;
        protected virtual void Awake()
        {
            this.TriggerStayingColliders = (this.triggerStayingColliders = new()).AsReadOnly();
            this.CollisionStayingColliders = (this.collisionStayingColliders = new()).AsReadOnly();

            this.core = this.CreateCore();
        }
        protected abstract IReliablePhysicsBridge<TCollider, TCollision> CreateCore();

        protected virtual void OnDisable() => this.ForceExitAll();
        protected virtual void OnDestroy() => this.ForceExitAll();
        private void ForceExitAll()
        {
            Process(this.triggerStayingColliders, this.TriggerStayingColliders, ColliderBuffer, this.core.OnReliableTriggerExitCallback_Internal, this.core.OnReliableTriggerStayingChangedCallback_Internal);
            Process(this.collisionStayingColliders, this.CollisionStayingColliders, ColliderBuffer, this.core.OnReliableCollisionExitCallback_Internal, this.core.OnReliableCollisionStayingChangedCallback_Internal);
            static void Process(
                HashSet<TCollider> staying, IReadOnlyHashSet<TCollider> readOnlyStaying,
                StaticBuffer<TCollider> buffer,
                System.Action<TCollider> onExitCallback,
                System.Action<IReadOnlyHashSet<TCollider>> onStayingChangedCallback)
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
                        onStayingChangedCallback(readOnlyStaying);
                        // And then fire exit callback.
                        onExitCallback(stay);
                    }
                }
                removeBuffer.ClearBuffer(removeCount);
            }
        }

        protected virtual void FixedUpdate()
        {
            Process(this.triggerStayingColliders, this.TriggerStayingColliders, ColliderBuffer, this.IsColliderEnabled, this.core.OnReliableTriggerExitCallback_Internal, this.core.OnReliableTriggerStayCallback_Internal, this.core.OnReliableTriggerStayingChangedCallback_Internal);
            Process(this.collisionStayingColliders, this.CollisionStayingColliders, ColliderBuffer, this.IsColliderEnabled, this.core.OnReliableCollisionExitCallback_Internal, this.core.OnReliableCollisionStayCallback_Internal, this.core.OnReliableCollisionStayingChangedCallback_Internal);
            static void Process(
                HashSet<TCollider> staying, IReadOnlyHashSet<TCollider> readOnlyStaying,
                StaticBuffer<TCollider> buffer,
                System.Func<TCollider, bool> isColliderEnabled,
                System.Action<TCollider> onExitCallback, System.Action<TCollider> onStayCallback,
                System.Action<IReadOnlyHashSet<TCollider>> onStayingChangedCallback)
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
                    onStayCallback(stay);
                }
                if (removeCount > 0)
                {
                    for (int i = 0; i < removeCount; i++)
                    {
                        var stay = removeBuffer.Buffer[i];
                        staying.Remove(stay);
                        // Always Fire staying changed callback first.
                        onStayingChangedCallback(readOnlyStaying);
                        // And then fire exit callback.
                        onExitCallback(stay);
                    }
                }
                removeBuffer.ClearBuffer(removeCount);
            }
        }

        #region Unity Physics Callbacks
        protected void OnUnityTriggerEnter(TCollider other)
            => this.OnTriggerEnterCallback_Internal(other);
        protected void OnUnityTriggerExit(TCollider other)
            => this.OnTriggerExitCallback_Internal(other);
        protected void OnUnityCollisionEnter(TCollision collision)
            => this.OnCollisionEnterCallback_Internal(collision);
        protected void OnUnityCollisionExit(TCollision collision)
            => this.OnCollisionExitCallback_Internal(collision);
        #endregion Unity Physics Callbacks

        #region Internal Physics Callbacks
        private void OnTriggerEnterCallback_Internal(TCollider other)
        {
            if (this.triggerStayingColliders.Add(other) == false) return;

            // Always Fire staying changed callback first.
            this.core.OnReliableTriggerStayingChangedCallback_Internal(this.TriggerStayingColliders);
            this.core.OnReliableTriggerEnterCallback_Internal(other);
        }
        private void OnTriggerExitCallback_Internal(TCollider other)
        {
            if (this.triggerStayingColliders.Remove(other) == false) return;

            // Always Fire staying changed callback first.
            this.core.OnReliableTriggerStayingChangedCallback_Internal(this.TriggerStayingColliders);
            this.core.OnReliableTriggerExitCallback_Internal(other);
        }

        private void OnCollisionEnterCallback_Internal(TCollision collision)
        {
            var collider = this.GetCollider(collision);
            if (this.collisionStayingColliders.Add(collider) == false) return;

            // Always Fire staying changed callback first.
            this.core.OnReliableCollisionStayingChangedCallback_Internal(this.CollisionStayingColliders);
            this.core.OnReliableCollisionEnterCallback_Internal(collider);
        }
        private void OnCollisionExitCallback_Internal(TCollision collision)
        {
            var collider = this.GetCollider(collision);
            if (this.collisionStayingColliders.Remove(collider) == false) return;

            // Always Fire staying changed callback first.
            this.core.OnReliableCollisionStayingChangedCallback_Internal(this.CollisionStayingColliders);
            this.core.OnReliableCollisionExitCallback_Internal(collider);
        }
        #endregion Internal Physics Callbacks
    }
    public interface IReliablePhysicsBridge<TCollider, TCollision>
    {
        void OnReliableTriggerEnterCallback_Internal(TCollider other);
        void OnReliableTriggerStayCallback_Internal(TCollider other);
        void OnReliableTriggerExitCallback_Internal(TCollider other);
        void OnReliableTriggerStayingChangedCallback_Internal(IReadOnlyHashSet<TCollider> staying);

        void OnReliableCollisionEnterCallback_Internal(TCollider other);
        void OnReliableCollisionStayCallback_Internal(TCollider other);
        void OnReliableCollisionExitCallback_Internal(TCollider other);
        void OnReliableCollisionStayingChangedCallback_Internal(IReadOnlyHashSet<TCollider> staying);
    }
}
