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

        private HashSet<Collider> triggerStayingColliders = null;
        private HashSet<Collider> collisionStayingColliders = null;

        private readonly static StaticBuffer<Collider> ColliderBuffer = new((_) => new Collider[ColliderBufferCapacity]);

        protected virtual void Awake()
        {
            this.triggerStayingColliders = new();
            this.collisionStayingColliders = new();
        }

        protected virtual void OnDisable() => this.ForceExitAll();
        protected virtual void OnDestroy() => this.ForceExitAll();
        private void ForceExitAll()
        {
            Process(this.triggerStayingColliders, ColliderBuffer, this.OnReliableTriggerExit);
            Process(this.collisionStayingColliders, ColliderBuffer, this.OnReliableCollisionExit);
            static void Process(HashSet<Collider> staying, StaticBuffer<Collider> buffer, System.Action<Collider> onExitCallback)
            {
                var removeCount = 0;
                var removeBuffer = buffer;
                foreach (var stay in staying) removeBuffer.AddItemSafely(ref removeCount, stay);

                for (int i = 0; i < removeCount; i++)
                {
                    var stay = removeBuffer.Buffer[i];
                    staying.Remove(stay);
                    onExitCallback(stay);
                }
                buffer.ClearBuffer(removeCount);
            }
        }

        protected virtual void FixedUpdate()
        {
            Process(this.triggerStayingColliders, ColliderBuffer, this.OnReliableTriggerExit, this.OnReliableTriggerStay);
            Process(this.collisionStayingColliders, ColliderBuffer, this.OnReliableCollisionExit, this.OnReliableCollisionStay);
            static void Process(HashSet<Collider> staying, StaticBuffer<Collider> buffer, System.Action<Collider> onExitCallback, System.Action<Collider> onStayCallback)
            {
                var removeCount = 0;
                var removeBuffer = buffer;
                foreach (var stay in staying)
                {
                    if (stay == null || stay.enabled == false || stay.gameObject.activeInHierarchy == false)
                    {
                        removeBuffer.AddItemSafely(ref removeCount, stay);
                        continue;
                    }
                    onStayCallback(stay);
                }
                for (int i = 0; i < removeCount; i++)
                {
                    var stay = removeBuffer.Buffer[i];
                    staying.Remove(stay);
                    onExitCallback(stay);
                }
                buffer.ClearBuffer(removeCount);
            }
        }

        #region Unity Physics Callbacks
        private void OnTriggerEnter(Collider other)
        {
            this.OnBaseTriggerEnter(other);
            this.OnTriggerEnterCallback_Internal(other);
        }
        private void OnTriggerExit(Collider other)
        {
            this.OnBaseTriggerExit(other);
            this.OnTriggerExitCallback_Internal(other);
        }
        private void OnCollisionEnter(Collision collision)
        {
            this.OnBaseCollisionEnter(collision);
            this.OnCollisionEnterCallback_Internal(collision);
        }
        private void OnCollisionExit(Collision collision)
        {
            this.OnBaseCollisionExit(collision);
            this.OnCollisionExitCallback_Internal(collision);
        }
        #endregion Unity Physics Callbacks

        #region Internal Physics Callbacks
        private void OnTriggerEnterCallback_Internal(Collider other)
        {
            if (this.triggerStayingColliders.Add(other) == false) return;
            this.OnReliableTriggerEnter(other);
        }
        private void OnTriggerExitCallback_Internal(Collider other)
        {
            if (this.triggerStayingColliders.Remove(other) == false) return;
            this.OnReliableTriggerExit(other);
        }
        private void OnCollisionEnterCallback_Internal(Collision collision)
        {
            if (this.collisionStayingColliders.Add(collision.collider) == false) return;
            this.OnReliableCollisionEnter(collision.collider);
        }
        private void OnCollisionExitCallback_Internal(Collision collision)
        {
            if (this.collisionStayingColliders.Remove(collision.collider) == false) return;
            this.OnReliableCollisionExit(collision.collider);
        }
        #endregion Internal Physics Callbacks

        #region Base Physics Callbacks
        protected virtual void OnBaseTriggerEnter(Collider other) { }
        protected virtual void OnBaseTriggerExit(Collider other) { }
        protected virtual void OnBaseCollisionEnter(Collision collision) { }
        protected virtual void OnBaseCollisionExit(Collision collision) { }
        #endregion Base Physics Callbacks

        #region Reliable Physics Callbacks
        protected virtual void OnReliableTriggerEnter(Collider other) { }
        protected virtual void OnReliableTriggerStay(Collider other) { }
        protected virtual void OnReliableTriggerExit(Collider other) { }
        protected virtual void OnReliableCollisionEnter(Collider other) { }
        protected virtual void OnReliableCollisionStay(Collider other) { }
        protected virtual void OnReliableCollisionExit(Collider other) { }
        #endregion Reliable Physics Callbacks
    }
}
