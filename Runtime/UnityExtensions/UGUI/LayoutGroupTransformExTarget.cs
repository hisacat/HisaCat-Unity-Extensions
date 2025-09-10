using UnityEngine;

namespace HisaCat.UGUI
{
    public class LayoutGroupTransformExTarget : MonoBehaviour
    {
        [SerializeField] private RectTransform m_Anchor = null;
        public RectTransform Anchor => this.m_Anchor;
    }
}
