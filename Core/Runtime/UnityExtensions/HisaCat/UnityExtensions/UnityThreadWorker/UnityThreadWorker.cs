using System.Collections.Generic;
using UnityEngine;

namespace HisaCat
{
    public class UnityThreadWorker : MonoBehaviour
    {
#if UNITY_EDITOR
#pragma warning disable IDE0051
        [UnityEditor.InitializeOnEnterPlayMode]
        private static void OnEnterPlaymodeInEditor(UnityEditor.EnterPlayModeOptions options)
        {
            if (options.HasFlag(UnityEditor.EnterPlayModeOptions.DisableDomainReload))
            {
                instance = null;
                mainThreadId = 0;
            }
        }
#pragma warning restore IDE0051
#endif

        private static UnityThreadWorker instance = null;

        private static bool IsMainThread => System.Threading.Thread.CurrentThread.ManagedThreadId == mainThreadId;
        private static int mainThreadId = 0;

        private readonly static Queue<System.Action> works = new();

        private void Awake()
        {
            if (instance != null)
            {
                Debug.LogError($"[{nameof(UnityThreadWorker)} {nameof(Awake)}: Instance already exists!");
                Destroy(gameObject);
                return;
            }

            mainThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId;

            this.transform.parent = null;
            DontDestroyOnLoad(this.gameObject);

            instance = this;
        }
        private void OnDestroy()
        {
            if (instance == this)
                instance = null;
        }
        private void Update()
        {
            lock (works)
            {
                while (works.Count > 0)
                    works.Dequeue().Invoke();
            }
        }

        public static void DelegateWork(System.Action action)
        {
            if (instance == null)
                throw new System.Exception($"{nameof(UnityThreadWorker)} is not exists. Please place {nameof(UnityThreadWorker)} prefab into initial scene.");

            if (IsMainThread)
            {
                action.Invoke();
                return;
            }

            lock (works) works.Enqueue(action);
        }
    }
}
