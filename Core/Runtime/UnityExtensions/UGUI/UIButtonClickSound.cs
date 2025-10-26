using HisaCat.Sounds;
using HisaCat.UnityExtensions;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace HisaCat.HUE.UI
{
    [RequireComponent(typeof(Button))]
    public class UIButtonClickSound : MonoBehaviour
    {
        [SerializeField] private Button m_Button = null;
        [SerializeField] private AudioClip m_AudioClip = null;
        [SerializeField] private float m_Volume = 1f;
        [SerializeField] private AudioMixerGroup m_AudioMixerGroup = null;
        protected virtual void Reset()
        {
            this.m_Button = GetComponent<Button>();
        }
        protected virtual void Awake()
        {
            if (this.m_Button == null) return;
            this.m_Button.onClick.AddListener(OnButtonClick);
        }

        protected virtual void OnButtonClick()
        {
            if (this.m_AudioClip == null) return;
            AudioSourceExtensions.PlayClipAtPoint(this.m_AudioClip, Vector3.zero, this.m_AudioMixerGroup, this.m_Volume, spatialBlend: 0f);
        }
    }
}
