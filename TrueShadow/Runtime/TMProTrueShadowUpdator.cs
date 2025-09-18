using UnityEngine;
using TMPro;
using LeTai.TrueShadow;
using HisaCat.PropertyAttributes;
using UnityEngine.UI;
using System.Collections.Generic;

namespace HisaCat.HUE
{
    [ExecuteAlways]
    [RequireComponent(typeof(TextMeshProUGUIEx))]
    [RequireComponent(typeof(TrueShadow))]
    public class TMProTrueShadowUpdator : MonoBehaviour
    {
        [ReadOnly][SerializeField] private TextMeshProUGUIEx m_TextMeshProUGUIEx = null;
        [ReadOnly][SerializeField] private TrueShadow m_TrueShadow = null;
        private HashSet<TMP_SubMeshUI> knownSubMeshes = new();

#if UNITY_EDITOR
        private void OnValidate() => TryInitializeOnEditor();
        private void Reset() => TryInitializeOnEditor();

        private void TryInitializeOnEditor()
        {
            if (Application.isPlaying) return;

            this.m_TextMeshProUGUIEx = this.GetComponent<TextMeshProUGUIEx>();
            this.m_TrueShadow = this.GetComponent<TrueShadow>();
        }
#endif
        private void Awake()
        {
#if UNITY_EDITOR
            TryInitializeOnEditor();
#endif
            this.m_TextMeshProUGUIEx.OnPostPopulateMesh -= OnPostPopulateMesh;
            this.m_TextMeshProUGUIEx.OnPostPopulateMesh += OnPostPopulateMesh;

            CopyToTMPSubMeshesIfRequired();
        }
        private void OnDestroy()
        {
            this.m_TextMeshProUGUIEx.OnPostPopulateMesh -= OnPostPopulateMesh;
        }

        private void OnPostPopulateMesh(VertexHelper vh) => CopyToTMPSubMeshesIfRequired();

        private void CopyToTMPSubMeshesIfRequired()
        {
            var subMeshObjects = this.m_TextMeshProUGUIEx.GetSubMeshObjects();
            foreach (var subMeshObject in subMeshObjects)
            {
                if (subMeshObject == null) continue;
                if (this.knownSubMeshes.Contains(subMeshObject)) continue;

                this.knownSubMeshes.Add(subMeshObject);
                this.m_TrueShadow.CopyTo(subMeshObject.gameObject);
            }

            this.knownSubMeshes.RemoveWhere(IsNull);
            static bool IsNull(TMP_SubMeshUI subMesh) => subMesh == null;
        }
    }
}
