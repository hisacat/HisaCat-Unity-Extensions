namespace HisaCat.DataBindEx.Setters.ActiveGroup
{
    using Slash.Unity.DataBind.Foundation.Setters;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    public class EnumGroupActiveSetterBase<TEnum> : EnumGroupActiveSetter<TEnum> where TEnum : struct
    {
        public List<EnumGameObjectPair> EnumGameObjectMapping;

        protected override GameObject GetGameObject(TEnum enumValue)
        {
            var enumGameObjectPair = this.EnumGameObjectMapping.FirstOrDefault(pair => pair.EnumValue.Equals(enumValue));
            return enumGameObjectPair != null ? enumGameObjectPair.GameObject : null;
        }

        protected override IEnumerable<GameObject> GetGameObjects()
        {
            return this.EnumGameObjectMapping.Select(pair => pair.GameObject).ToList();
        }

        [Serializable]
        public class EnumGameObjectPair
        {
            public TEnum EnumValue;
            public GameObject GameObject;
        }
    }
}
