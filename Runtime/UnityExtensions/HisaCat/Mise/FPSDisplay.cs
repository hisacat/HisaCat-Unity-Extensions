using UnityEngine;

namespace HisaCat.Mise
{
    public class FPSDisplay : MonoBehaviour
    {
        [Tooltip("Number of frames to sample for averaging.")]
        [SerializeField] private int sampleSize = 30; // 샘플링할 프레임 수

        private float[] frameDurations;
        private int sampleIndex = 0;
        private float totalFrameTime = 0;

        private void Awake()
        {
            this.frameDurations = new float[sampleSize];

            DontDestroyOnLoad(this.gameObject);
        }

        private void Update()
        {
            float currentDeltaTime = Time.unscaledDeltaTime;
            this.totalFrameTime -= this.frameDurations[this.sampleIndex];
            this.totalFrameTime += currentDeltaTime;
            this.frameDurations[this.sampleIndex] = currentDeltaTime;
            this.sampleIndex = (this.sampleIndex + 1) % this.sampleSize;
        }

        private GUIStyle style = new();
        private void OnGUI()
        {
            float averageDeltaTime = totalFrameTime / sampleSize;
            float fps = 1.0f / averageDeltaTime;

            float scale = Screen.height / 1080.0f;

            this.style.fontSize = (int)(20 * scale);
            this.style.normal.textColor = Color.white;

            Rect rect = new(10 * scale, 10 * scale, Screen.width, Screen.height * 0.05f);
            GUI.Label(rect, $"FPS: {fps:0.0}", style);
        }
    }
}
