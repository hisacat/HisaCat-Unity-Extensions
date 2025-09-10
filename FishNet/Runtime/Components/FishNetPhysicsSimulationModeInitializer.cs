#if FISHNET
using FishNet.Managing;
using FishNet.Managing.Timing;
using UnityEngine;

namespace HisaCat.FishNet.Components
{
    /// <summary>
    /// FishNet's TimeManager modifies <see cref="Physics.simulationMode"/> to <see cref="SimulationMode.Script"/> based on its configuration.<br/>
    /// It is designed to reset to <see cref="SimulationMode.FixedUpdate"/> during application shutdown.<br/>
    /// However, due to what seems to be a bug in Unity6, the reset value is not properly saved to the Project Settings.<br/>
    /// Additionally, TimeManager does not reset this value if it is deleted during runtime.<br/>
    /// <br/>
    /// This class resets <see cref="Physics.simulationMode"/> to <see cref="SimulationMode.FixedUpdate"/> during the game's initial load and when the TimeManager is deleted.<br/>
    /// This ensures that the initial value of simulationMode is safely maintained as FixedUpdate.
    /// See <see href="https://gitea.nas.hisa.cat/DopamineBank/HappyDoor/wiki/Unity-Physics-%EB%B3%80%EA%B2%BD%EC%82%AC%ED%95%AD-%EB%B0%8F-%ED%8A%B8%EB%9F%AC%EB%B8%94%EC%8A%88%ED%8C%85"/>
    /// </summary>
    public class FishNetPhysicsSimulationModeInitializer : MonoBehaviour
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
        public static FishNetPhysicsSimulationModeInitializer Instance { get; private set; } = null;

        public const SimulationMode InitialSimulationMode = SimulationMode.FixedUpdate;
        public const SimulationMode2D InitialSimulationMode2D = SimulationMode2D.FixedUpdate;

        private static void InitializeSimulationMode(string from)
        {
            Debug.Log($"<b>Set Physics.simulationMode to {InitialSimulationMode} from {from}</b>");
            Physics.simulationMode = InitialSimulationMode;
            Debug.Log($"<b>Set Physics2D.simulationMode to {InitialSimulationMode2D} from {from}</b>");
            Physics2D.simulationMode = InitialSimulationMode2D;
        }

        [RuntimeInitializeOnLoadMethod]
        private static void RuntimeInitializeOnLoad()
        {
            // If TimeManager already exists and activated, do nothing.
            // The TimeManager will manage simulationMode internally
            var timeManager = FindTimeManagerWithoutLog();
            if (timeManager == null || timeManager.isActiveAndEnabled == false)
                InitializeSimulationMode("Initialize ");

            if (Instance == null)
            {
                var gameobject = new GameObject(nameof(FishNetPhysicsSimulationModeInitializer));
                Instance = gameobject.AddComponent<FishNetPhysicsSimulationModeInitializer>();
                DontDestroyOnLoad(gameobject);
            }
        }
        private static TimeManager FindTimeManagerWithoutLog()
        {
            if (NetworkManager.Instances.Count <= 0) return null;
            var networkManager = NetworkManager.Instances[0];
            return networkManager.TimeManager;
        }


        private bool wasTimeManagerExists = false;
        private TimeManager lastTimeManager = null;

        private void Awake()
        {
            this.lastTimeManager = FindTimeManagerWithoutLog();
            this.wasTimeManagerExists = this.lastTimeManager != null;
        }

        private void Update()
        {
            if (this.wasTimeManagerExists)
            {
                if (this.lastTimeManager == null)
                {
                    InitializeSimulationMode("TimeManager destroyed");
                    lastTimeManager = null;
                    this.wasTimeManagerExists = false;
                }
            }
            else
            {
                this.lastTimeManager = FindTimeManagerWithoutLog();
                if (this.lastTimeManager != null)
                {
                    this.wasTimeManagerExists = true;
                }
            }
        }
    }
}
#endif
