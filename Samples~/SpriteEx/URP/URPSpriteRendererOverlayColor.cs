using HisaCat.PropertyAttributes;
using UnityEngine;

namespace HisaCat.Shaders.URP.U2D
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SpriteRenderer))]
    public class SpriteRendererOverlayColor : MonoBehaviour
    {
        public const string DefaultMaterialGUID = "72df459c977e5ea4195159f6046d0663";

        [ReadOnly][SerializeField] private SpriteRenderer m_SpriteRenderer = null;
        public Color OverlayColor
        {
            get => this.m_OverlayColor;
            set
            {
                this.m_OverlayColor = value;
                ApplyColorIfDirty();
            }
        }
        [SerializeField] private Color m_OverlayColor = new(1, 1, 1, 0);

        private Color lastAppliedColor = new(1, 1, 1, 0);
        private const string OverlayColorPropertyName = "_OverlayColor";
        private static readonly int OverlayColorId = Shader.PropertyToID(OverlayColorPropertyName);
        private MaterialPropertyBlock mpb = null;

        private void Reset()
        {
#if UNITY_EDITOR
            if (this.m_SpriteRenderer == null)
            {
                this.m_SpriteRenderer = GetComponent<SpriteRenderer>();
                UnityEditor.EditorUtility.SetDirty(this);
            }

            this.ApplyColor();
#endif
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (IsValidShader() == false)
            {
                var material = UnityEditor.AssetDatabase.LoadAssetByGUID<Material>(new UnityEditor.GUID(DefaultMaterialGUID));
                Log($"Update material to {material.name} automatically.");
                this.m_SpriteRenderer.sharedMaterial = material;
            }

            this.ApplyColor();
        }
#endif

        private void Awake()
        {
#if UNITY_EDITOR
            if (Application.isPlaying == false) return;
#endif

            if (IsValidShader() == false)
            {
                Log($"Current shader doesn't have \"{OverlayColorPropertyName}\" property." +
                    $"\r\nPlease set valid shader.");
                return;
            }

            this.ApplyColor();
        }

        private void OnDestroy()
        {
            if (IsValidShader() == false) return;

            this.m_OverlayColor = new(1, 1, 1, 0);
            ApplyColorIfDirty();
        }

        private void OnDidApplyAnimationProperties()
        {
            if (IsValidShader() == false) return;

            ApplyColorIfDirty();
        }

        private bool IsValidShader()
        {
            if (this.m_SpriteRenderer.sharedMaterial == null) return false;
            if (this.m_SpriteRenderer.sharedMaterial.shader == null) return false;
            if (this.m_SpriteRenderer.sharedMaterial.shader.FindPropertyIndex(OverlayColorPropertyName) <= 0) return false;
            return true;
        }

        private void ApplyColorIfDirty()
        {
            if (this.lastAppliedColor != this.m_OverlayColor)
            {
                this.ApplyColor();
                this.lastAppliedColor = this.m_OverlayColor;
            }
        }

        private void ApplyColor()
        {
            if (this.mpb == null) this.mpb = new MaterialPropertyBlock();

            this.m_SpriteRenderer.GetPropertyBlock(this.mpb);
            this.mpb.SetColor(OverlayColorId, this.m_OverlayColor);
            this.m_SpriteRenderer.SetPropertyBlock(this.mpb);
        }

        #region Logs
        private static void Log(string message) => Debug.Log($"[{nameof(SpriteRendererOverlayColor)}] {message}");
        #endregion Logs
    }
}
