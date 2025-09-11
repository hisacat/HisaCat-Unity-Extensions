using HisaCat.PropertyAttributes;
using UnityEngine;

namespace HisaCat.Shaders.URP.U2D
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SpriteRenderer))]
    public class SpriteRendererOverlayColor : MonoBehaviour
    {
        public const string ShaderName = "HisaCat/URP/Sprite-Lit-OverlayColor-Default";
        public const string DefaultMaterialGUID = "72df459c977e5ea4195159f6046d0663";

        [ReadOnly][SerializeField] private SpriteRenderer m_SpriteRenderer = null;
        [SerializeField] private Color m_OverlayColor = new(1, 1, 1, 0);

        private Color lastAppliedColor = new(1, 1, 1, 0);
        private static readonly int OverlayColorId = Shader.PropertyToID("_OverlayColor");
        private MaterialPropertyBlock mpb = null;

        private void Awake()
        {
#if UNITY_EDITOR
            if (this.m_SpriteRenderer == null)
            {
                this.m_SpriteRenderer = GetComponent<SpriteRenderer>();
                UnityEditor.EditorUtility.SetDirty(this);
            }
#endif

            if (IsValidShader() == false)
            {
                Debug.Log($"[{nameof(SpriteRendererOverlayColor)}] Material shader must be \"{ShaderName}\".");
                return;
            }

            this.ApplyColor();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (IsValidShader() == false)
            {
                var material = UnityEditor.AssetDatabase.LoadAssetByGUID<Material>(new UnityEditor.GUID(DefaultMaterialGUID));
                Debug.Log($"[{nameof(SpriteRendererOverlayColor)}] Update material to {material.name} automatically.");
                this.m_SpriteRenderer.sharedMaterial = material;
            }

            this.ApplyColor();
        }
#endif

        private bool IsValidShader()
        {
            if (this.m_SpriteRenderer.sharedMaterial == null) return false;
            if (this.m_SpriteRenderer.sharedMaterial.shader == null) return false;
            if (this.m_SpriteRenderer.sharedMaterial.shader.name != ShaderName) return false;
            return true;
        }

        private void OnPreRender()
        {
            if (IsValidShader() == false) return;

            if (this.lastAppliedColor != this.m_OverlayColor)
            {
                this.ApplyColor();
                this.lastAppliedColor = this.m_OverlayColor;
            }
        }

        private void OnDestroy()
        {
            if (IsValidShader() == false) return;

            this.m_OverlayColor = new(1, 1, 1, 0);
            this.lastAppliedColor = this.m_OverlayColor;
            this.ApplyColor();
        }

        private void ApplyColor()
        {
            if (this.mpb == null) this.mpb = new MaterialPropertyBlock();

            this.m_SpriteRenderer.GetPropertyBlock(this.mpb);
            this.mpb.SetColor(OverlayColorId, this.m_OverlayColor);
            this.m_SpriteRenderer.SetPropertyBlock(this.mpb);
        }
    }
}
