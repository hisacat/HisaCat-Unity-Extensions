using FishNet;
using FishNet.Managing.Scened;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnitySceneManager = UnityEngine.SceneManagement.SceneManager;

namespace HisaCat.FishNet.Components
{
    /// <summary>
    /// This component temporarily fixes an issue in FishNet ~v4.5.8R<br/>
    /// where the <b>Active Scene is not set correctly</b>.<br/>
    /// It is intended to be added to the <b>root GameObject of the initial entry scene.</b><br/>
    /// <br/>
    /// This script subscribes to the Scene loaded events from both FishNet's <see cref="SceneManager"/><br/>
    /// resetting the Active Scene to the last scene loaded through FishNet.<br/>
    /// <br/>
    /// Bug Description:<br/>
    /// When loading a scene using <see cref="SceneManager.LoadConnectionScenes"/>:<br/>
    /// * On the host, the loaded scene is correctly set as the Active Scene.<br/>
    /// * However, on other clients, the Active Scene is incorrectly set to <b>"MovedObjectHolder"</b>.<br/>
    /// <see href="https://discord.com/channels/424284635074134018/1034477094731784302/1334220796490678396">Discord link</see><br/>
    /// <see href="https://gitea.nas.hisa.cat/DopamineBank/HappyDoor/issues/76">Gitea Issue</see><br/>
    /// </summary>
    public class ActiveSceneFixer : MonoBehaviour
    {
#if UNITY_EDITOR
#pragma warning disable IDE0051
        [UnityEditor.InitializeOnEnterPlayMode]
        private static void OnEnterPlaymodeInEditor(UnityEditor.EnterPlayModeOptions options)
        {
            if (options.HasFlag(UnityEditor.EnterPlayModeOptions.DisableDomainReload))
            {
                Instance = null;
            }
        }
#pragma warning restore IDE0051
#endif

        #region Singleton
        public static ActiveSceneFixer Instance { get; private set; } = null;
        private void Awake()
        {
            if (Instance != null)
            {
                Debug.LogError($"[{nameof(ActiveSceneFixer)}] Instance already exists!");
                Destroy(this.gameObject);
                return;
            }

            Instance = this;
            Init();
        }
        private void OnDestroy()
        {
            if (Instance == null)
            {
                Instance = null;
                Dispose();
            }
        }
        #endregion Singleton

        /// <summary>
        /// True to automatically set active scenes when loading and unloading scenes.
        /// </summary>
        [Tooltip("True to automatically set active scenes when loading and unloading scenes.")]
        [SerializeField]
        private bool m_setActiveScene = true;

        private void Init()
        {
            if (InstanceFinder.SceneManager != null)
            {
                InstanceFinder.SceneManager.OnUnloadEnd += SceneManagerOnUnloadEnd;
                InstanceFinder.SceneManager.OnLoadEnd += SceneManagerOnLoadEnd;
            }

            DontDestroyOnLoad(this.gameObject);
        }
        private void Dispose()
        {
            if (InstanceFinder.SceneManager != null)
            {
                InstanceFinder.SceneManager.OnUnloadEnd -= SceneManagerOnUnloadEnd;
                InstanceFinder.SceneManager.OnLoadEnd -= SceneManagerOnLoadEnd;
            }
        }

        private void SceneManagerOnUnloadEnd(SceneUnloadEndEventArgs obj)
        {
            if (this.m_setActiveScene == false) return;

            if (InstanceFinder.NetworkManager == null)
            {
                Debug.LogError($"[{nameof(ActiveSceneFixer)}] NetworkManager missing!");
                return;
            }

            var sld = InstanceFinder.NetworkManager.IsServerStarted && InstanceFinder.NetworkManager.IsHostStarted == false ?
                obj.QueueData.SceneUnloadData.PreferredActiveScene.Server : obj.QueueData.SceneUnloadData.PreferredActiveScene.Client;
            if (sld == null) return;
            var scene = sld.GetScene(out _);
            if (scene.IsValid() == false) return;

            UnitySceneManager.SetActiveScene(scene);
            ManagedDebug.Log($"[{nameof(ActiveSceneFixer)}] {nameof(SceneManagerOnUnloadEnd)}: Set active scene as \"{scene.name}\"");
        }

        private void SceneManagerOnLoadEnd(SceneLoadEndEventArgs obj)
        {
            if (this.m_setActiveScene == false) return;

            if (obj.LoadedScenes.Length <= 0) return;
            var lastScene = obj.LoadedScenes[^1];
            if (lastScene.IsValid() == false) return;

            UnitySceneManager.SetActiveScene(lastScene);
            ManagedDebug.Log($"[{nameof(ActiveSceneFixer)}] {nameof(SceneManagerOnLoadEnd)}: Set active scene as \"{lastScene.name}\"");
        }
    }
}
