using System;
using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace HisaCat.HUE.UnityExtensions
{
    /// <summary>
    /// Unity의 <see cref="MonoBehaviour.StopCoroutine(Coroutine)"/>는 코루틴 실행을 중단할 뿐,
    /// 컴파일러가 생성한 이터레이터의 <see cref="IDisposable.Dispose"/>를 호출하지 않는다.
    /// 그 결과 코루틴을 도중에 멈추면 <c>try/finally</c>(및 <c>using</c>)의 정리 구문이 실행되지 않는다.
    /// <para>
    /// <see cref="DisposableCoroutine"/>는 실행 중인 <see cref="Coroutine"/> 핸들과 원본 <see cref="IEnumerator"/>를
    /// 함께 보관하여, <see cref="Dispose"/> 시 코루틴을 중단함과 동시에 이터레이터를 직접 Dispose한다.
    /// 이로써 중단 시점에도 finally 정리 구문이 확실히 실행되도록 보장한다.
    /// </para>
    /// <remarks>
    /// 주의: GameObject 파괴/비활성화로 코루틴이 멈추는 경우엔 이 경로를 거치지 않으므로 finally는 실행되지 않는다.
    /// 이런 경우까지 정리가 필요하면 OnDisable/OnDestroy에서 별도로 <see cref="Dispose"/>를 호출해야 한다.
    /// </remarks>
    /// </summary>
    public class DisposableCoroutine : IDisposable
    {
        /// <summary>
        /// <see cref="MonoBehaviour.StartCoroutine(IEnumerator)"/>가 반환한 실행 핸들.
        /// 코루틴이 첫 tick에 즉시 완료되는 등의 경우 null일 수 있다.
        /// </summary>
        public Coroutine Coroutine { get; private set; } = null;
        /// <summary>
        /// 코루틴의 원본 이터레이터. <see cref="Dispose"/> 대상이며, 이를 통해 finally 정리 구문이 실행된다.
        /// </summary>
        public IEnumerator Enumerator { get; private set; } = null;

        public MonoBehaviour Owner { get; private set; } = null;
        public bool IsDisposed { get; private set; } = false;

        internal DisposableCoroutine(MonoBehaviour owner, Coroutine coroutine, IEnumerator enumerator)
        {
            this.Owner = owner;
            this.Coroutine = coroutine;
            this.Enumerator = enumerator;
        }

        /// <summary>
        /// 코루틴 실행을 중단하고 이터레이터를 Dispose하여 finally 정리 구문을 실행한다.
        /// 여러 번 호출해도 안전하다(멱등).
        /// </summary>
        public void StopAndDispose() => ((IDisposable)this).Dispose();
        void IDisposable.Dispose()
        {
            if (this.IsDisposed) return;
            this.IsDisposed = true;

            if (this.Owner != null && this.Coroutine != null)
                this.Owner.StopCoroutine(this.Coroutine);
            this.Owner = null; this.Coroutine = null;

            // 이미 완주/Dispose된 이터레이터에 대한 Dispose는 no-op이므로 안전함.
            if (this.Enumerator != null)
                (this.Enumerator as IDisposable)?.Dispose();
            this.Enumerator = null;
        }
    }

    public static class DisposableCoroutineExtensions
    {
        /// <summary>
        /// <see cref="MonoBehaviour.StartCoroutine(IEnumerator)"/>로 코루틴을 시작하고,
        /// 중단 시 finally 정리 구문 실행을 보장할 수 있도록 <see cref="DisposableCoroutine"/>으로 감싸 반환한다.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DisposableCoroutine StartDisposableCoroutine(this MonoBehaviour monoBehaviour, IEnumerator enumerator)
            => new(monoBehaviour, monoBehaviour.StartCoroutine(enumerator), enumerator);
    }
}
