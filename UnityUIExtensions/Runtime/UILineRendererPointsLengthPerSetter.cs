#if UNITY_UI_EXTENSIONS
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI.Extensions;

namespace HisaCat.UIExtensions
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(UILineRenderer))]
    public class UILineRendererPointsLengthPerSetter : MonoBehaviour
    {
        private UILineRenderer _target = null;
        public UILineRenderer Target
        {
            get
            {
                if (this._target == null)
                    this._target = GetComponent<UILineRenderer>();
                return this._target;
            }
        }

        public Vector2[] Points
        {
            get => this.m_Points;
            set
            {
                this.m_Points = value;
                UpdatePoints();
            }
        }
        [SerializeField] private Vector2[] m_Points = null;

        public float LengthPer
        {
            get => this.m_LengthPer;
            set
            {
                this.m_LengthPer = value;
                UpdatePoints();
            }
        }
        [SerializeField][Range(0f, 1f)] private float m_LengthPer = 0;

        private void Awake()
        {
            ApplyModifiedValues();
        }

        /// <summary>
        /// Use this instead 'LateUpdate() => UpdatePoints()'.
        /// This is Called from the native side any time a animation property is changed.
        /// https://forum.unity.com/threads/help-please-with-animation-component-public-properties-custom-inspector.229328/
        /// https://qiita.com/amagitakayosi/items/4dcd5c05b7f6b09a7f3f
        /// https://docs.unity3d.com/Packages/com.unity.ugui@1.0/manual/UIAutoLayout.html
        /// </summary>
        private void OnDidApplyAnimationProperties() => ApplyModifiedValues();
        private void OnValidate() => ApplyModifiedValues();
        private void ApplyModifiedValues()
        {
            UpdatePoints();
        }

        private void UpdatePoints()
        {
            Vector2[] result = default;

            if (this.m_LengthPer <= 0)
            {
                result = null;
                result = new Vector2[] { Vector2.zero };
            }
            else if (this.m_LengthPer >= 1)
            {
                result = this.m_Points;
            }
            else
            {
                var length = 0f;
                for (int i = 1; i < this.m_Points.Length; i++)
                    length += Vector2.Distance(this.m_Points[i - 1], this.m_Points[i]);

                var leftLength = length * this.m_LengthPer;
                var _result = new List<Vector2>();
                _result.Add(this.m_Points[0]);
                for (int i = 1; i < this.m_Points.Length; i++)
                {
                    var curLength = Vector2.Distance(this.m_Points[i - 1], this.m_Points[i]);
                    if (leftLength >= curLength)
                    {
                        _result.Add(this.m_Points[i]);
                        leftLength -= curLength;
                    }
                    else
                    {
                        _result.Add(Vector2.MoveTowards(this.m_Points[i - 1], this.m_Points[i], leftLength));
                        break;
                    }
                }

                result = _result.ToArray();
            }

            Target.Points = result;
#if UNITY_EDITOR
            //For update mesh based on changed points.
            if (Application.isPlaying == false) Target.SetAllDirty();
#endif
        }
    }
}
#endif
