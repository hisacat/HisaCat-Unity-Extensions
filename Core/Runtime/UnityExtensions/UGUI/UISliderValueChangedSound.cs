using HisaCat.Sounds;
using HisaCat.HUE.UnityExtensions;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace HisaCat.HUE.UI
{
    [RequireComponent(typeof(Slider))]
    public class UISliderValueChangedSound : MonoBehaviour
    {
        [SerializeField] private Slider m_Slider = null;
        [SerializeField] private AudioClip m_AudioClip = null;
        [SerializeField] private float m_Volume = 1f;
        [SerializeField] private AudioMixerGroup m_AudioMixerGroup = null;
        [SerializeField] private float m_MinimumPlayAudioInterval = 0.1f;
        [SerializeField] private bool m_DontPlayAudioOnStart = true;

        private int startFrameCount = 0;
        private float m_LastAudioPlayedTime = 0f;
        protected virtual void Reset()
        {
            this.m_Slider = GetComponent<Slider>();
        }
        protected virtual void Awake()
        {
            if (this.m_Slider == null) return;
            this.m_Slider.onValueChanged.AddListener(OnSliderValueChanged);
        }
        protected virtual void Start()
        {
            this.startFrameCount = Time.frameCount;
        }
        protected virtual void OnDestroy()
        {
            if (this.m_Slider == null) return;
            this.m_Slider.onValueChanged.RemoveListener(OnSliderValueChanged);
        }
        protected virtual void OnSliderValueChanged(float value)
        {
            if (this.m_AudioClip == null) return;
            if (this.m_DontPlayAudioOnStart && Time.frameCount == this.startFrameCount) return;
            if (Time.realtimeSinceStartup - this.m_LastAudioPlayedTime < this.m_MinimumPlayAudioInterval) return;

            AudioSourceExtensions.PlayClipAtPoint(this.m_AudioClip, Vector3.zero, this.m_AudioMixerGroup, this.m_Volume, spatialBlend: 0f);
            this.m_LastAudioPlayedTime = Time.realtimeSinceStartup;
        }
    }
}
