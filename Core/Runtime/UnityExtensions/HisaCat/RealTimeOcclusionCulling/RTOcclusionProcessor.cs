using HisaCat.HUE.UnityExtensions;
using System.Collections.Generic;
using UnityEngine;

namespace HisaCat.RealTimeOcclusionCulling
{
    public class RTOcclusionProcessor : MonoBehaviour
    {
#if UNITY_EDITOR
#pragma warning disable IDE0051
        [UnityEditor.InitializeOnEnterPlayMode]
        private static void OnEnterPlaymodeInEditor(UnityEditor.EnterPlayModeOptions options)
        {
            if (options.HasFlag(UnityEditor.EnterPlayModeOptions.DisableDomainReload))
            {
                instance = null;
            }
        }
#pragma warning restore IDE0051
#endif

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void OnAfterSceneLoad()
        {
            CreateInstance();
        }

        private static RTOcclusionProcessor instance = null;
        private static bool CreateInstance()
        {
            if (ConditionLog.LogError(Application.isPlaying == false, "Cannot create instance in edit mode!"))
                return false;

            if (ConditionLog.LogError(instance != null, "Instance already exists!"))
                return false;

            var go = new GameObject($"[{nameof(RTOcclusionProcessor)}]");
            instance = go.AddComponent<RTOcclusionProcessor>();
            DontDestroyOnLoad(go);
            return true;
        }

        private void Start()
        {
            if (instance != this)
            {
                Debug.LogError($"[{nameof(RTOcclusionProcessor)}] Instance already exists! It will be destroyed.");
                Destroy(this.gameObject);
                return;
            }
            Initialize();
        }
        private void OnDestroy()
        {
            if (instance == null)
            {
                instance = null;
                Dispose();
            }
        }

        private void Initialize() { }
        private void Dispose() { }

        protected virtual void LateUpdate()
        {
            #pragma warning disable CS0162 // Unreachable code detected
            if (RTOcclusionManager.IsActivated)
                RTOcclusionManager.UpdateCullingIfRequired();
            #pragma warning restore CS0162 // Unreachable code detected
        }
    }
}
