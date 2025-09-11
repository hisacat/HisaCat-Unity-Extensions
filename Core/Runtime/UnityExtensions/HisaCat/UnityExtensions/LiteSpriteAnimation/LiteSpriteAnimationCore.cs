using UnityEngine;
using HisaCat.PropertyAttributes;

namespace HisaCat
{
    public abstract class LiteSpriteAnimationCore : MonoBehaviour
    {
        public Sprite[] FrameSprites { get => this.m_FrameSprites; set => this.m_FrameSprites = value; }
        [SerializeField] private Sprite[] m_FrameSprites = null;

        public bool PlayAutomatically { get => this.m_PlayAutomatically; set => this.m_PlayAutomatically = value; }
        [SerializeField] private bool m_PlayAutomatically = true;
        public float FrameRate { get => this.m_FrameRate; set => this.m_FrameRate = value; }
        [Min(0)][SerializeField] private float m_FrameRate = 60f;
        public bool IsLooping { get => this.m_IsLooping; set => this.m_IsLooping = value; }
        [SerializeField] private bool m_IsLooping = false;

        public bool IsPlaying { get; private set; } = false;

        /// <summary>
        /// Use Time.unscaledTime instead Time.deltaTime?
        /// </summary>
        public bool UseUnscaledTime { get => m_UseUnscaledTime; set => this.m_UseUnscaledTime = value; }
        [SerializeField] private bool m_UseUnscaledTime = false;
        /// <summary>
        /// The playback speed of the animation. 1 is normal playback speed.
        /// A negative playback speed will play the animation backwards.
        /// </summary>
        public float Speed { get => this.m_Speed; set => this.m_Speed = value; }
        [SerializeField] private float m_Speed = 1f;

        /// <summary>
        /// The current time of the animation.
        /// </summary>
        public float Time { get => this.m_Time; set => this.m_Time = value; }
        [ReadOnly][SerializeField] private float m_Time = 0;
        /// <summary>
        /// The normalized time of the animation.
        /// </summary>
        public float NormalizedTime { get => this.m_NormalizedTime; set => this.m_NormalizedTime = value; }
        [ReadOnly][SerializeField] private float m_NormalizedTime = 0;
        /// <summary>
        /// The length of the animation clip in seconds.
        /// </summary>
        public float Length => this.m_FrameSprites.Length / this.m_FrameRate;

        #region Core
        private void Update()
        {
            if (this.IsPlaying)
            {
                this.m_Time += (this.m_UseUnscaledTime ? UnityEngine.Time.unscaledDeltaTime : UnityEngine.Time.deltaTime) * this.m_Speed;
                this.m_NormalizedTime = this.m_Time / this.Length;

                Sample();
            }
        }

        public void Sample()
        {
            int frameIdx;

            if (this.m_IsLooping)
                frameIdx = (int)(this.m_FrameSprites.Length * Mathf.Abs(this.m_NormalizedTime)) % this.m_FrameSprites.Length;
            else
                frameIdx = Mathf.Min((int)(this.m_FrameSprites.Length * Mathf.Abs(this.m_NormalizedTime)), this.m_FrameSprites.Length - 1);

            ApplySprite(this.m_FrameSprites[frameIdx]);
        }

        protected abstract void ApplySprite(Sprite sprite);
        #endregion

        public void Play()
        {
            this.m_Time = this.m_NormalizedTime = 0f; //Reset time
            this.IsPlaying = true;

#if UNITY_EDITOR
            if (this.m_FrameSprites.Length <= 0)
                Debug.LogError("[LiteSpriteAnimator] Play: Sprite frame length null!");
            if (this.m_FrameRate <= 0)
                Debug.LogError("[LiteSpriteAnimator] Play: Sprite FrameRate cannot be zero!");
#endif
        }
        public void Stop()
        {
            this.IsPlaying = false;
        }

        private void OnEnable()
        {
            if (this.m_PlayAutomatically) Play();
        }
        private void OnDisable()
        {
            Stop();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (Application.isPlaying == false && this.IsPlaying == false)
            {
                if (this.m_FrameSprites.Length > 1)
                    ApplySprite(this.m_FrameSprites[0]);
            }
        }
#endif
    }
}
