#if HUE_UGUI_TMPRO_INCLUDED
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace HisaCat.HUE
{
    [AddComponentMenu("HisaCat/HUE/UI/TextMeshPro - Text (UI) Extensions")]
    public class TextMeshProUGUIEx : TextMeshProUGUI
    {
        public delegate void OnPopulateMeshDelegate(VertexHelper vh);
        public event OnPopulateMeshDelegate OnPrePopulateMesh = null, OnPostPopulateMesh = null;

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            this.OnPrePopulateMesh?.Invoke(vh);

            base.OnPopulateMesh(vh);

            this.OnPostPopulateMesh?.Invoke(vh);
        }

        public TMP_SubMeshUI[] GetSubMeshObjects() => this.m_subTextObjects;
    }
}
#endif
