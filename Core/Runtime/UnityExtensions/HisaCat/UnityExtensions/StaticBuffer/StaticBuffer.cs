using UnityEngine;

namespace HisaCat
{
    /// <summary>
    /// 정적 버퍼 클래스.<br/>
    /// Unity 에디터의 Domain Reload 시에도 안전하게 초기화할 수 있는 정적 배열 버퍼를 제공합니다.
    /// </summary>
    /// <typeparam name="T">버퍼에 저장할 데이터 타입</typeparam>
    /// <remarks>
    /// <para>
    /// 이 클래스는 Unity 에디터에서 Play Mode 진입 시 발생하는 Domain Reload와 관련된 문제를 해결하기 위해 설계되었습니다.<br/>
    /// 특히 <see cref="UnityEditor.InitializeOnEnterPlayMode"/> 속성과 함께 사용하여<br/>
    /// Domain Reload가 비활성화된 상태에서도 정적 버퍼를 안전하게 초기화할 수 있습니다.
    /// </para>
    /// <para>
    /// 사용 예시:
    /// <code>
    /// // 정적 버퍼 선언
    /// private static StaticBuffer&lt;RaycastHit&gt; RaycastHitsBuffer = 
    ///     new((_) => new RaycastHit[128]);
    /// 
    /// // Domain Reload 시 초기화 (InitializeOnEnterPlayMode와 함께 사용)
    /// [UnityEditor.InitializeOnEnterPlayMode]
    /// private static void OnEnterPlaymodeInEditor(UnityEditor.EnterPlayModeOptions options)
    /// {
    ///     if (options.HasFlag(UnityEditor.EnterPlayModeOptions.DisableDomainReload))
    ///     {
    ///         RaycastHitsBuffer.Initialize();
    ///     }
    /// }
    /// </code>
    /// </para>
    /// </remarks>
    public class StaticBuffer<T>
    {
        /// <summary>
        /// 실제 데이터를 저장하는 배열 버퍼
        /// </summary>
        public T[] Buffer => buffer;
        private T[] buffer = null;

        /// <summary>
        /// 버퍼 초기화를 담당하는 델리게이트
        /// </summary>
        private readonly System.Func<T[], T[]> Initializer;

        /// <summary>
        /// StaticBuffer 인스턴스를 생성하고 초기화합니다.
        /// </summary>
        /// <param name="initializer">버퍼 초기화를 위한 델리게이트. 기존 버퍼를 입력받아 새로운 버퍼를 반환합니다.</param>
        public StaticBuffer(System.Func<T[], T[]> initializer)
        {
            this.Initializer = initializer;
            Initialize();
        }

        /// <summary>
        /// 버퍼를 초기화합니다. Initializer 델리게이트를 호출하여 새로운 버퍼를 생성합니다.
        /// </summary>
        public void Initialize() => this.buffer = this.Initializer(this.buffer);

        /// <summary>
        /// 버퍼에 아이템을 추가하고 인덱스를 증가시킵니다.<br/>
        /// 버퍼 크기가 부족하면 자동으로 크기를 증가합니다.
        /// </summary>
        /// <param name="index">추가할 인덱스</param>
        /// <param name="element">추가할 아이템</param>
        public void AddItemSafely(ref int index, T element)
        {
            if (this.Buffer.Length <= index)
            {
                Debug.LogWarning($"[{nameof(StaticBuffer<T>)}]: Buffer capacity reached. Automatically increasing buffer size. This may impact performance.");
                System.Array.Resize(ref this.buffer, this.buffer.Length * 2);
            }
            this.Buffer[index++] = element;
        }

        /// <summary>
        /// 전체 버퍼 요소를 초기화합니다.
        /// </summary>
        public void ClearBuffer() => this.ClearBuffer(this.Buffer.Length);
        /// <summary>
        /// 버퍼 요소를 앞에서부터 주어진 갯수만큼 초기화합니다.
        /// </summary>
        /// <param name="count">초기화할 요소 개수</param>
        public void ClearBuffer(int count)
        {
            if (count <= 0) return; // Do nothing.
            if (count > this.buffer.Length) count = this.buffer.Length; // Clamp.
            System.Array.Clear(this.Buffer, 0, count);
        }
    }
}
