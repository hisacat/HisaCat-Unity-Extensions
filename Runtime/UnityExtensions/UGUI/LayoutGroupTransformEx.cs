using UnityEngine;

namespace HisaCat.UGUI
{
    [ExecuteAlways]
    public class LayoutGroupTransformEx : MonoBehaviour
    {
        [SerializeField] private bool m_ModifyPositionY = false;
        [SerializeField] private AnimationCurve m_PositionYCurve = null;
        [SerializeField] private bool m_Mirror = false;

        bool isDirty = false;
        private void OnEnable()
        {
            this.isDirty = true;
        }

        private void OnTransformChildrenChanged()
        {
            this.isDirty = true;
        }

        private void OnValidate()
        {
            this.isDirty = true;
        }

        private void LateUpdate()
        {
            if (this.isDirty)
            {
                this.UpdateTransform();
                this.isDirty = false;
            }
        }

        public void UpdateTransform()
        {
            var children = GetComponentsInChildren<LayoutGroupTransformExTarget>();
            int childCount = children.Length;
            for (int i = 0; i < childCount; i++)
            {
                if (this.m_ModifyPositionY)
                {
                    var child = children[i];
                    var position = child.Anchor.anchoredPosition;

                    if (childCount <= 1)
                    {
                        position.y = m_PositionYCurve.Evaluate(0);
                    }
                    else
                    {
                        if (this.m_Mirror)
                        {
                            int firstHalfCount = Mathf.CeilToInt(childCount / 2f);
                            int secondHalfCount = childCount - firstHalfCount;

                            float t;
                            if (i < firstHalfCount)
                            {
                                t = (firstHalfCount > 1) ? (float)i / (firstHalfCount - 1) : 0;
                            }
                            else
                            {
                                int indexInSecond = i - firstHalfCount;
                                t = (secondHalfCount > 1) ? (float)(secondHalfCount - 1 - indexInSecond) / (secondHalfCount - 1) : 0;
                            }

                            position.y = m_PositionYCurve.Evaluate(t);
                        }
                        else
                        {
                            position.y = m_PositionYCurve.Evaluate((float)i / (childCount - 1));
                        }
                    }

                    child.Anchor.anchoredPosition = position;
                }
            }
        }
    }
}
