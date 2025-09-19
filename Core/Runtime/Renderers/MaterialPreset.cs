using UnityEngine;

namespace HisaCat.Renderers
{
    [System.Serializable]
    public class MaterialPresets
    {
        public MaterialPreset[] Presets => this.m_Presets;
        [SerializeField] private MaterialPreset[] m_Presets = null;


        [System.Serializable]
        public class MaterialPreset
        {
            public TargetRenderer[] Targets => this.m_Targets;
            [SerializeField] private TargetRenderer[] m_Targets = null;
            public Material Material => this.m_Material;
            [SerializeField] private Material m_Material = null;

            [System.Serializable]
            public class TargetRenderer
            {
                public Renderer Renderer => this.m_Renderer;
                [SerializeField] private Renderer m_Renderer = null;
                public int MaterialIndex => this.m_MaterialIndex;
                [SerializeField] private int m_MaterialIndex = 0;
            }

            public void ApplyPreset()
            {
                foreach (var target in this.m_Targets)
                {
                    var materials = target.Renderer.sharedMaterials;
                    var index = target.MaterialIndex;
                    if (index < 0 || index >= materials.Length)
                    {
                        Debug.LogError($"Material index is out of range: {index}");
                        continue;
                    }
                    materials[index] = this.Material;
                    target.Renderer.sharedMaterials = materials;
                }
            }
        }

        public void ApplyPresets()
        {
            foreach (var preset in this.m_Presets)
                preset.ApplyPreset();
        }
    }
}
