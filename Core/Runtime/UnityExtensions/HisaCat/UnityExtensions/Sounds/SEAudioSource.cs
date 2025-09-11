using UnityEngine;

namespace HisaCat.Sounds
{
    public class SEAudioSource : MonoBehaviour
    {
        public static SEAudioSource Instantiate(AudioClip clip, float volume, Vector2 position, Transform parent)
        {
            var go = new GameObject();
            go.transform.position = position;
            var component = go.AddComponent<SEAudioSource>();
            component.Initialize(clip, volume);
            return component;
        }

        private AudioSource target = null;
        public AudioSource Target => this.target;
        private float clipVolume = 0;
        public void Initialize(AudioClip clip, float clipVolume)
        {
            this.target = GetComponent<AudioSource>();
            this.target.clip = clip;
            this.target.volume = clipVolume;

            UpdateSEVolume();
        }
        private void Awake()
        {
            SoundManager.OnSEVolumeChanged += OnSEVolumeChanged;
        }
        private void OnDestroy()
        {
            SoundManager.OnSEVolumeChanged -= OnSEVolumeChanged;
        }
        private void Start()
        {
            this.clipVolume = this.target.volume;
            this.target.Play();
        }

        private void OnSEVolumeChanged(float volume)
        {
            UpdateSEVolume();
        }
        private void UpdateSEVolume()
        {
            this.target.volume = this.clipVolume * SoundManager.SEVolume;
        }

        public void SetClipVolume(float volume)
        {
            this.clipVolume = volume;
        }
    }
}
