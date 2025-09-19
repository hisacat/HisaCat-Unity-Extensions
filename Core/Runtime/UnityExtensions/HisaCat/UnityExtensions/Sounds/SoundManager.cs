using System.Collections;
using System.Collections.Generic;
using HisaCat.Collections;
using HisaCat.SimpleObjectPool;
using HisaCat.Utilities;
using UnityEngine;

namespace HisaCat.Sounds
{
    public class SoundManager : MonoBehaviour
    {
#if UNITY_EDITOR
#pragma warning disable IDE0051
        [UnityEditor.InitializeOnEnterPlayMode]
        private static void OnEnterPlaymodeInEditor(UnityEditor.EnterPlayModeOptions options)
        {
            if (options.HasFlag(UnityEditor.EnterPlayModeOptions.DisableDomainReload))
            {
                OnBGMVolumeChanged = null;
                OnSEVolumeChanged = null;
                _SEVolume = 1f;
                _BGMVolume = 1f;
                _instance = null;
                currentAudioListner = null;
            }
        }
#pragma warning restore IDE0051
#endif

        public static readonly IReadOnlyList<float> SupportSEPitch = new List<float>()
        {
            1.05f, 1.1f, 1.15f,
            0.95f, 0.9f, 0.85f
        };

        public const float BGMReferenceVolume = 0.55f;
        public const float DefaultBlockSECollapseTime = 0.02f;

        public delegate void VolumeChangedEventHandler(float volume);
        public static event VolumeChangedEventHandler OnBGMVolumeChanged;
        public static event VolumeChangedEventHandler OnSEVolumeChanged;

        private static float _SEVolume = 1f;
        public static float SEVolume
        {
            get => _SEVolume;
            set
            {
                _SEVolume = value;
                if (instance != null)
                {
                    UpdateSEVolume();
                    OnSEVolumeChanged?.Invoke(_SEVolume);
                }
            }
        }
        private static void UpdateSEVolume()
        {
            instance.audioSource_SE.volume = SEVolume;
        }
        private static float _BGMVolume = 1f;
        public static float BGMVolume
        {
            get => _BGMVolume;
            set
            {
                _BGMVolume = value;
                if (instance != null)
                {
                    UpdateBGMVolume();
                    OnBGMVolumeChanged?.Invoke(_BGMVolume);
                }
            }
        }
        private static void UpdateBGMVolume()
        {
            instance.audioSource_BGM.volume = BGMReferenceVolume * BGMVolume;
        }

        private static SoundManager _instance = null;
        private static SoundManager instance
        {
            get
            {
                if (_instance == null)
                {
                    if (ApplicationUtils.IsQuitting()) return null;

                    var go = new GameObject("[HisaCat.Sounds.SoundManager]");
                    _instance = go.AddComponent<SoundManager>();
                    _instance.Initialize();
                    GameObject.DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        public const int ManagedSEAudioSourceCacheCount = 64;
        private void Awake()
        {
            // var audioSource = new GameObject("[Managed Sound Effect]").AddComponent<AudioSource>();
            // SimpleObjectPoolManager.AddPool<AudioSource>(audioSource, ManagedSEAudioSourceCacheCount);
        }
        private void OnDestroy()
        {
            if (_instance == this)
                _instance = null;
        }

        private AudioSource audioSource_BGM = null;
        private AudioSource audioSource_SE = null;
        private Dictionary<int, AudioSource> audioSource_PitchSEDic = null; //Key is index of SupportSEPitch.

        private Dictionary<AudioClip, float> collapseTimeTablePerClip = null;

        public class BGMTicket
        {
            public readonly object Owner = null;
            public readonly AudioClip Clip = null;
            public BGMTicket(object owner, AudioClip clip)
            {
                this.Owner = owner;
                this.Clip = clip;
            }
        }

        private List<BGMTicket> bgmQueue = null;

        private void Initialize()
        {
            var bgmGo = new GameObject("BGM");
            bgmGo.transform.parent = this.transform;
            this.audioSource_BGM = bgmGo.AddComponent<AudioSource>();
            this.audioSource_BGM.spatialBlend = 0; //Force 2D

            var seGO = new GameObject("SE");
            seGO.transform.parent = this.transform;
            this.audioSource_SE = seGO.AddComponent<AudioSource>();
            this.audioSource_SE.spatialBlend = 0; //Force 2D

            this.audioSource_PitchSEDic = new Dictionary<int, AudioSource>();
            var pitchCount = SupportSEPitch.Count;
            for (int i = 0; i < pitchCount; i++)
            {
                var pitchAudioSource = seGO.AddComponent<AudioSource>();
                pitchAudioSource.pitch = SupportSEPitch[i];
                pitchAudioSource.spatialBlend = 0; //Force 2D
                this.audioSource_PitchSEDic.Add(i, pitchAudioSource);
            }

            UpdateBGMVolume();
            UpdateSEVolume();

            this.collapseTimeTablePerClip = new Dictionary<AudioClip, float>();
            this.bgmQueue = new List<BGMTicket>();
        }

        public static BGMTicket PlayBGMFromQueue(object owner, AudioClip clip)
        {
            if (instance == null) return null;

            if (clip == null)
            {
                ManagedDebug.LogError($"[{nameof(SoundManager)}] {nameof(PlayBGMFromQueue)}: clip is null");
                return null;
            }

            var ticket = new BGMTicket(owner, clip);
            instance.bgmQueue.Add(ticket);

            UpdateBGMFromQueue();
            return ticket;
        }
        public static BGMTicket PauseBGMFromQueue(object owner)
        {
            if (instance == null) return null;

            var key = new object();

            var ticket = new BGMTicket(owner, null);
            instance.bgmQueue.Add(ticket);

            UpdateBGMFromQueue();
            return ticket;
        }
        public static bool RemoveBGMFromQueue(BGMTicket ticket)
        {
            if (instance == null) return false;

            if (ticket == null || instance.bgmQueue.Contains(ticket) == false)
                return false;

            instance.bgmQueue.Remove(ticket);

            UpdateBGMFromQueue();
            return true;
        }
        public static void StopAndClearBGMQueue()
        {
            if (instance == null) return;

            instance.bgmQueue.Clear();

            UpdateBGMFromQueue();
        }
        private static void UpdateBGMFromQueue()
        {
            if (instance == null) return;

            if (instance.bgmQueue.Count <= 0)
            {
                instance.audioSource_BGM.Stop();
                instance.audioSource_BGM.clip = null;
            }
            else
            {
                var currentBGMTicket = instance.bgmQueue[instance.bgmQueue.Count - 1];
                var currentBGMClip = currentBGMTicket.Clip;

                //If clip changed in same frame, then isPlaying is still true. so if clip is changed, It think of IsPlaying is false.
                if (instance.audioSource_BGM.clip != currentBGMClip || instance.audioSource_BGM.isPlaying == false)
                {
                    instance.audioSource_BGM.clip = currentBGMClip;

                    instance.audioSource_BGM.loop = true;
                    instance.audioSource_BGM.Play();
                }
            }
        }

        private static AudioListener currentAudioListner = null;
        private static Vector3 ActiveAudioListnerPosition
        {
            get
            {
                if (currentAudioListner == null)
                    currentAudioListner = FindFirstObjectByType<AudioListener>();

                return currentAudioListner == null ? Vector3.zero : currentAudioListner.transform.position;
            }
        }


        public static void PlaySE(AudioClip clip, float clipVolume = 1f, float collapse = DefaultBlockSECollapseTime, AudioClip collapseReferenceClip = null) =>
            PlaySE(instance.audioSource_SE, clip, clipVolume, collapse, collapseReferenceClip);
        public static void PlaySERandomPitch(AudioClip clip, float clipVolume = 1f, float collapse = DefaultBlockSECollapseTime, AudioClip collapseReferenceClip = null) =>
            PlaySE(instance.audioSource_PitchSEDic[Random.Range(0, SupportSEPitch.Count)], clip, clipVolume, collapse, collapseReferenceClip);
        private static void PlaySE(AudioSource audioSource, AudioClip clip, float clipVolume, float collapse, AudioClip collapseReferenceClip)
        {
            if (instance == null) return;

            if (clip == null) return;

            //Sets collapse time to dont play same sound until collapse time.
            if (collapse > 0)
            {
                if (collapseReferenceClip == null) collapseReferenceClip = clip;
                if (instance.collapseTimeTablePerClip.ContainsKey(collapseReferenceClip) && Time.unscaledTime - instance.collapseTimeTablePerClip[collapseReferenceClip] < collapse)
                    return;
                instance.collapseTimeTablePerClip[collapseReferenceClip] = Time.unscaledTime;
            }

            //If position not defined. use SE AudioSource instead AudioSource.PlayClipAtPoint for better performance.
            audioSource.PlayOneShot(clip, clipVolume * SEVolume);
        }
        public static void PlaySEAtPoint(AudioClip clip, Vector3 position, float clipVolume = 1f, float collapse = DefaultBlockSECollapseTime, AudioClip collapseReferenceClip = null)
        {
            if (instance == null) return;

            if (clip == null) return;

            //Sets collapse time to dont play same sound until collapse time.
            if (collapse > 0)
            {
                if (collapseReferenceClip == null) collapseReferenceClip = clip;
                if (instance.collapseTimeTablePerClip.ContainsKey(collapseReferenceClip) && Time.unscaledTime - instance.collapseTimeTablePerClip[collapseReferenceClip] < collapse)
                    return;
                instance.collapseTimeTablePerClip[collapseReferenceClip] = Time.unscaledTime;
            }

            AudioSource.PlayClipAtPoint(clip, position, clipVolume * SEVolume);
        }

        public static AudioSource PlaySEManaged(AudioClip clip, float clipVolume, bool destroyOnComplete) { return PlaySEManaged(clip, clipVolume, destroyOnComplete, ActiveAudioListnerPosition); }
        public static AudioSource PlaySEManaged(AudioClip clip, float clipVolume, bool destroyOnComplete, Vector3 position)
        {
            if (instance == null) return null;

            if (clip == null) return null;

            var target = SEAudioSource.Instantiate(clip, clipVolume, position, instance.transform);
            instance.StartCoroutine(WaitForSecondsAction(clip.length, () => GameObject.Destroy(target.gameObject)));
            return target.Target;
        }

        private static IEnumerator WaitForSecondsAction(float seconds, System.Action action)
        {
            yield return CachedYieldInstruction.WaitForSeconds(seconds);
            action.Invoke();
        }
    }
}
