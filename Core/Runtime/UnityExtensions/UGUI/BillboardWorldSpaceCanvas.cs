using HisaCat.PropertyAttributes;
using HisaCat.HUE.UnityExtensions;
using UnityEngine;

namespace HisaCat.UGUI
{
    [ExecuteAlways]
    [RequireComponent(typeof(Canvas))]
    public class BillboardWorldSpaceCanvas : MonoBehaviour
    {
        [ReadOnly][SerializeField] private Canvas m_Canvas = null;
        [SerializeField] private Quaternion m_Rotation = Quaternion.identity;
        private void Awake()
        {
            if (this.ConditionLogWarning(this.m_Canvas == null,
            $"[{nameof(BillboardWorldSpaceCanvas)}] {nameof(Canvas)} is not assigned! it will be assigned automatically.", this))
                this.m_Canvas = GetComponent<Canvas>();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            this.m_Canvas = GetComponent<Canvas>();
        }
#endif

        private void LateUpdate()
        {
            this.transform.rotation = Camera.main.transform.rotation * this.m_Rotation;
        }
    }
}
