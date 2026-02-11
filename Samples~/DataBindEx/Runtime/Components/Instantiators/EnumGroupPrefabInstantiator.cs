namespace HisaCat.HUE.DataBindEx.Instantiators.EnumGroup
{
    using System;
    using System.Collections.Generic;
    using Slash.Unity.DataBind.Core.Presentation;
    using UnityEngine;

    public abstract class EnumGroupPrefabInstantiator<TEnum> : DataBindingOperator where TEnum : Enum
    {
        /// <summary>
        ///     Provider for enum value.
        /// </summary>
        [DataTypeHintGenericType]
        public DataBinding EnumValue;

        /// <summary>
        ///     Target to get the data from.
        /// </summary>
        [DataTypeHintExplicit(typeof(Transform))]
        public DataBinding TargetBinding;

        public GroupInstantiator[] m_GroupInstantiators = null;

        [System.Serializable]
        public class GroupInstantiator
        {
            public List<TEnum> EnumValues;
            public GameObject Prefab;

            [NonSerialized] public GameObject Instance = null;
        }

        public override void Deinit()
        {
            base.Deinit();

            this.RemoveBinding(this.EnumValue);
            this.RemoveBinding(this.TargetBinding);
        }

        public override void Init()
        {
            base.Init();

            this.AddBinding(this.EnumValue);
            this.AddBinding(this.TargetBinding);
        }

        public override void Disable()
        {
            base.Disable();

            this.EnumValue.ValueChanged -= this.OnEnumValueChanged;
            this.TargetBinding.ValueChanged -= this.OnTargetChanged;
        }

        public override void Enable()
        {
            base.Enable();

            this.EnumValue.ValueChanged += this.OnEnumValueChanged;
            this.TargetBinding.ValueChanged += this.OnTargetChanged;

            if (this.EnumValue.IsInitialized) this.OnEnumValueChanged();
            if (this.TargetBinding.IsInitialized) this.OnTargetChanged();
        }

        protected virtual void OnEnumValueChanged()
        {
            this.UpdateInstance();
        }

        protected virtual void OnTargetChanged()
        {
            this.UpdateInstance();
        }

        private void UpdateInstance()
        {
            if (this.EnumValue.IsInitialized == false || this.TargetBinding.IsInitialized == false) return;

            foreach (var item in this.m_GroupInstantiators)
            {
                if (item.EnumValues.Contains(this.EnumValue.GetValue<TEnum>()))
                {
                    var _parent = this.TargetBinding.Value;
                    if (_parent != null && _parent is not Transform)
                        Debug.LogError($"Parent is type is not {nameof(Transform)}. current type is {_parent.GetType().Name}");
                    var parent = (Transform)_parent;

                    if (item.Instance == null)
                    {
                        if (item.Prefab != null)
                            item.Instance = Instantiate(item.Prefab, parent);
                    }
                    else
                    {
                        if (item.Instance.transform.parent != parent)
                            item.Instance.transform.SetParent(parent, false);
                    }
                }
                else
                {
                    if (item.Instance != null)
                    {
                        Destroy(item.Instance);
                        item.Instance = null;
                    }
                }
            }
        }
    }
}
