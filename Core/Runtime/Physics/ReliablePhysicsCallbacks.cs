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
    public abstract class ReliablePhysicsCallbacks : MonoBehaviour
    {
#if UNITY_EDITOR
#pragma warning disable IDE0051
        [UnityEditor.InitializeOnEnterPlayMode]
        private static void OnEnterPlaymodeInEditor(UnityEditor.EnterPlayModeOptions options)
        {
            if (options.HasFlag(UnityEditor.EnterPlayModeOptions.DisableDomainReload))
            {
                ColliderBuffer.Initialize();
            }
        }
#pragma warning restore IDE0051
#endif

        private const int ColliderBufferCapacity = 1024;
        private readonly static StaticBuffer<Collider> ColliderBuffer = new((_) => new Collider[ColliderBufferCapacity]);

        public IReadOnlyHashSet<Collider> TriggerStayingColliders { get; private set; } = null;
        private HashSet<Collider> triggerStayingColliders = null;
        public IReadOnlyHashSet<Collider> CollisionStayingColliders { get; private set; } = null;
        private HashSet<Collider> collisionStayingColliders = null;

        protected virtual void Awake()
        {
            this.TriggerStayingColliders = (this.triggerStayingColliders = new()).AsReadOnly();
            this.CollisionStayingColliders = (this.collisionStayingColliders = new()).AsReadOnly();
        }

        protected virtual void OnDisable() => this.ForceExitAll();
        protected virtual void OnDestroy() => this.ForceExitAll();
        private void ForceExitAll()
        {
            Process(this.triggerStayingColliders, this.TriggerStayingColliders, ColliderBuffer, this.OnReliableTriggerExitCallback, this.OnReliableTriggerStayingChangedCallback);
            Process(this.collisionStayingColliders, this.CollisionStayingColliders, ColliderBuffer, this.OnReliableCollisionExitCallback, this.OnReliableCollisionStayingChangedCallback);
            static void Process(
                HashSet<Collider> staying, IReadOnlyHashSet<Collider> readOnlyStaying,
                StaticBuffer<Collider> buffer,
                System.Action<Collider> onExitCallback,
                System.Action<IReadOnlyHashSet<Collider>> onStayingChangedCallback)
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
            Process(this.triggerStayingColliders, this.TriggerStayingColliders, ColliderBuffer, this.OnReliableTriggerExitCallback, this.OnReliableTriggerStayCallback, this.OnReliableTriggerStayingChangedCallback);
            Process(this.collisionStayingColliders, this.CollisionStayingColliders, ColliderBuffer, this.OnReliableCollisionExitCallback, this.OnReliableCollisionStayCallback, this.OnReliableCollisionStayingChangedCallback);
            static void Process(
                HashSet<Collider> staying, IReadOnlyHashSet<Collider> readOnlyStaying,
                StaticBuffer<Collider> buffer,
                System.Action<Collider> onExitCallback, System.Action<Collider> onStayCallback,
                System.Action<IReadOnlyHashSet<Collider>> onStayingChangedCallback)
            {
                var removeCount = 0;
                var removeBuffer = buffer;
                foreach (var stay in staying)
                {
                    // Remove invalid colliders.
                    if (stay == null || stay.enabled == false || stay.gameObject.activeInHierarchy == false)
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
        private void OnTriggerEnter(Collider other)
        {
            this.OnBaseTriggerEnterCallback(other);
            this.OnTriggerEnterCallback_Internal(other);
        }
        private void OnTriggerExit(Collider other)
        {
            this.OnBaseTriggerExitCallback(other);
            this.OnTriggerExitCallback_Internal(other);
        }
        private void OnCollisionEnter(Collision collision)
        {
            this.OnBaseCollisionEnterCallback(collision);
            this.OnCollisionEnterCallback_Internal(collision);
        }
        private void OnCollisionExit(Collision collision)
        {
            this.OnBaseCollisionExitCallback(collision);
            this.OnCollisionExitCallback_Internal(collision);
        }
        #endregion Unity Physics Callbacks

        #region Internal Physics Callbacks
        private void OnTriggerEnterCallback_Internal(Collider other)
        {
            if (this.triggerStayingColliders.Add(other) == false) return;

            // Always Fire staying changed callback first.
            this.OnReliableTriggerStayingChangedCallback(this.TriggerStayingColliders);
            this.OnReliableTriggerEnterCallback(other);
        }
        private void OnTriggerExitCallback_Internal(Collider other)
        {
            if (this.triggerStayingColliders.Remove(other) == false) return;

            // Always Fire staying changed callback first.
            this.OnReliableTriggerStayingChangedCallback(this.TriggerStayingColliders);
            this.OnReliableTriggerExitCallback(other);
        }

        private void OnCollisionEnterCallback_Internal(Collision collision)
        {
            if (this.collisionStayingColliders.Add(collision.collider) == false) return;

            // Always Fire staying changed callback first.
            this.OnReliableCollisionStayingChangedCallback(this.CollisionStayingColliders);
            this.OnReliableCollisionEnterCallback(collision.collider);
        }
        private void OnCollisionExitCallback_Internal(Collision collision)
        {
            if (this.collisionStayingColliders.Remove(collision.collider) == false) return;

            // Always Fire staying changed callback first.
            this.OnReliableCollisionStayingChangedCallback(this.CollisionStayingColliders);
            this.OnReliableCollisionExitCallback(collision.collider);
        }
        #endregion Internal Physics Callbacks

        #region Base Physics Callbacks
        protected virtual void OnBaseTriggerEnterCallback(Collider other) { }
        protected virtual void OnBaseTriggerExitCallback(Collider other) { }

        protected virtual void OnBaseCollisionEnterCallback(Collision collision) { }
        protected virtual void OnBaseCollisionExitCallback(Collision collision) { }
        #endregion Base Physics Callbacks

        #region Reliable Physics Callbacks
        protected virtual void OnReliableTriggerEnterCallback(Collider other) { }
        protected virtual void OnReliableTriggerStayCallback(Collider other) { }
        protected virtual void OnReliableTriggerExitCallback(Collider other) { }
        protected virtual void OnReliableTriggerStayingChangedCallback(IReadOnlyHashSet<Collider> staying) { }

        protected virtual void OnReliableCollisionEnterCallback(Collider other) { }
        protected virtual void OnReliableCollisionStayCallback(Collider other) { }
        protected virtual void OnReliableCollisionExitCallback(Collider other) { }
        protected virtual void OnReliableCollisionStayingChangedCallback(IReadOnlyHashSet<Collider> staying) { }
        #endregion Reliable Physics Callbacks
    }
}
