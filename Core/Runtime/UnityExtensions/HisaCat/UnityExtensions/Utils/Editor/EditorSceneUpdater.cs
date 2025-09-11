using UnityEditor;
using UnityEngine;

namespace HisaCat.Editors
{
    public class EditorSceneUpdater : Editor
    {
#pragma warning disable IDE0051
        [UnityEditor.InitializeOnEnterPlayMode]
        private static void OnEnterPlaymodeInEditor(UnityEditor.EnterPlayModeOptions options)
        {
            if (options.HasFlag(UnityEditor.EnterPlayModeOptions.DisableDomainReload))
            {
                IsInitialized = false;
                IsOn = false;
            }
        }
#pragma warning restore IDE0051

        private static bool IsInitialized = false;
        private static bool IsOn = false;

        [MenuItem("HisaCat/Editor Scene Updater/Turn On")]
        public static void EditorUpdateOn()
        {
            EditorUpdateSet(true);
        }
        [MenuItem("HisaCat/Editor Scene Updater/Turn Off")]
        public static void EditorUpdateOff()
        {
            EditorUpdateSet(false);
        }

        private static void Initialize()
        {
            if (IsInitialized)
                return;

            EditorApplication.update += OnUpdate;
            SceneView.duringSceneGui += DuringSceneGui;

            IsInitialized = true;
        }

        public static void EditorUpdateSet(bool isOn)
        {
            if (isOn && Application.isPlaying)
            {
                Debug.LogError("[EditorSceneUpdater] Cannot turn on when editor is playing!");
                return;
            }

            if (IsInitialized == false) Initialize();
            IsOn = isOn;
        }

        private static void OnUpdate()
        {
            if (IsOn == false) return;

            EditorApplication.QueuePlayerLoopUpdate();
        }

        private static void DuringSceneGui(SceneView sceneview)
        {
            if (IsOn == false) return;

            Handles.BeginGUI();
            {
                GUI.color = Color.red;
                GUI.Label(new Rect(0, 0, 300f, 20f), "EditorSceneUpdator launched!");
                GUI.color = Color.white;
                if (GUI.Button(new Rect(0, 20f, 100f, 20f), "(Click to turn off)"))
                {
                    EditorUpdateOff();
                }
            }
            Handles.EndGUI();
        }
    }
}
