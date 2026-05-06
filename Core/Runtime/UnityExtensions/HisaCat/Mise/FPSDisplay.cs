using HisaCat.HUE.UnityExtensions;
using UnityEngine;

namespace HisaCat.Mise
{
    public class FPSDisplay : MonoBehaviour
    {
        [SerializeField] private bool m_DontDestroyOnLoad = false;

        [Tooltip("Number of frames to sample for averaging.")]
        [SerializeField] private int m_SampleSize = 30; // 샘플링할 프레임 수
        [SerializeField] private float m_FontSize = 20f;
        [SerializeField] private Color m_TextColor = Color.white;
        [SerializeField] private Color m_BackgroundColor = Color.black.WithAlpha(0.5f);

        private float[] frameDurations;
        private int sampleIndex = 0;
        private float totalFrameTime = 0;

        private void Awake()
        {
            this.frameDurations = new float[this.m_SampleSize];

            if (this.m_DontDestroyOnLoad)
                DontDestroyOnLoad(this.gameObject);
        }

        private void Update()
        {
            float currentDeltaTime = Time.unscaledDeltaTime;
            this.totalFrameTime -= this.frameDurations[this.sampleIndex];
            this.totalFrameTime += currentDeltaTime;
            this.frameDurations[this.sampleIndex] = currentDeltaTime;
            this.sampleIndex = (this.sampleIndex + 1) % this.m_SampleSize;
        }

        private readonly GUIStyle backgroundStyle = new();
        private readonly GUIStyle labelStyle = new();

        private float lastScale = 0;
        private bool contentChanged = true;
        private Vector2 textSize = Vector2.zero;
        private GUIContent contentForCalcSize = null;

#if UNITY_EDITOR
        void OnValidate() => this.contentChanged = true;
#endif
        private void OnGUI()
        {
            float averageDeltaTime = this.totalFrameTime / this.m_SampleSize;
            float fps = 1.0f / averageDeltaTime;

            float scale = Screen.height / 1080.0f;
            if (this.lastScale != scale)
            {
                this.contentChanged = true;
                this.lastScale = scale;
            }

            this.backgroundStyle.normal.background = Texture2D.whiteTexture;
            GUI.backgroundColor = this.m_BackgroundColor;

            int scaledFontSize = Mathf.RoundToInt(this.m_FontSize * scale);
            this.labelStyle.fontSize = scaledFontSize;
            this.labelStyle.normal.textColor = this.m_TextColor;


            if (this.contentChanged)
            {
                // For the maximum width to be secured,
                // calculate the size with the maximum number of digits.
                this.textSize = this.labelStyle.CalcSize(
                    this.contentForCalcSize ??= new GUIContent(formatFps(999))
                );

                this.contentChanged = false;
            }

            float padding = 5 * scale;
            (float width, float height) = (this.textSize.x, this.textSize.y);

            // Draw background
            {
                Rect rect = new(0, 0, width + padding * 2, height + padding * 2);
                GUI.Box(rect, string.Empty, this.backgroundStyle);
            }

            // Draw text
            {
                Rect rect = new(padding, padding, width, height);
                GUI.Label(rect, formatFps(fps), this.labelStyle);
            }

            string formatFps(float fps) => $"FPS: {fps:0.0}";
        }
    }
}
