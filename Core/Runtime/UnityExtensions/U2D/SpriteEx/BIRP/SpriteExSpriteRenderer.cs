using HisaCat.UnityExtensions;
using HisaCat.PropertyAttributes;
using UnityEngine;

namespace HisaCat.SpriteEx
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class SpriteExSpriteRenderer : MonoBehaviour
    {
#if UNITY_EDITOR
#pragma warning disable IDE0051
        [UnityEditor.InitializeOnEnterPlayMode]
        private static void OnEnterPlaymodeInEditor(UnityEditor.EnterPlayModeOptions options)
        {
            if (options.HasFlag(UnityEditor.EnterPlayModeOptions.DisableDomainReload))
            {
                _SpriteExShader = null;
                _SpriteExMaterial = null;
            }
        }
#pragma warning restore IDE0051
#endif

        private const string SpriteExAssetRootPath = "Assets/HisaCat/SpriteEx";
        public const string SpriteExShaderName = "HisaCat/Sprite/SpriteEx";
        public const string SpriteExMaterialResourcePath = "HisaCat/SpriteEx/Sprites-SpriteEx";

        private static Shader _SpriteExShader = null;
        public static Shader FindSpriteExShader() => _SpriteExShader == null ? _SpriteExShader = Shader.Find(SpriteExShaderName) : _SpriteExShader;

        private static Material _SpriteExMaterial = null;
        public static Material LoadSpriteExMaterial() => _SpriteExMaterial == null ? _SpriteExMaterial = Resources.Load<Material>("HisaCat/SpriteEx/Sprites-SpriteEx") : _SpriteExMaterial;

        public SpriteRenderer Renderer => this.m_SpriteRenderer;
        [ReadOnly][SerializeField] private SpriteRenderer m_SpriteRenderer = null;

        [SerializeField] private Color m_FillColor = Color.clear;

#if UNITY_EDITOR
        private void Reset()
        {
            this.m_SpriteRenderer = this.GetComponent<SpriteRenderer>();
            if (this.m_SpriteRenderer.sharedMaterial == null || this.m_SpriteRenderer.sharedMaterial.shader != FindSpriteExShader())
                this.m_SpriteRenderer.sharedMaterial = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(SpriteExAssetRootPath + "/Resources/" + SpriteExMaterialResourcePath + ".mat");
        }
#endif

        MaterialPropertyBlock prop = null;
        private void Awake()
        {
#if UNITY_EDITOR
            if (this.m_SpriteRenderer.sharedMaterial == null || this.m_SpriteRenderer.sharedMaterial.shader != FindSpriteExShader())
            {
                Debug.LogError($"<b>[SpriteRendererEx] This renderer's shader is not SpriteExShader!</b>" +
                    $"\npath: {this.transform.GetFullPath()}" +
                    "\nSpriteEx not working collectly. Set material with LoadSpriteExMaterial or FindSpriteExShader." +
                    "\n(This error only shown on editor.)");
            }
#endif
            this.prop = new MaterialPropertyBlock();
            this.Renderer.GetPropertyBlock(prop);

            this.prop.SetColor("_FillColor", this.m_FillColor);
            this.Renderer.SetPropertyBlock(prop);
        }

        public Color fillColor
        {
            get => this.m_FillColor;
            set
            {
                this.m_FillColor = value;
                this.prop.SetColor("_FillColor", this.m_FillColor);
                this.Renderer.SetPropertyBlock(prop);
            }
        }

        public Color color
        {
            get => this.Renderer.color;
            set => this.Renderer.color = value;
        }

        private void OnValidate()
        {
            if (prop == null)
            {
                this.prop = new MaterialPropertyBlock();
                this.Renderer.GetPropertyBlock(prop);
            }

            this.prop.SetColor("_FillColor", this.m_FillColor);
            this.Renderer.SetPropertyBlock(prop);
        }
    }
}
