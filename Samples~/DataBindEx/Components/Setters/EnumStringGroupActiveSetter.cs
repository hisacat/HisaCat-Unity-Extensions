namespace HisaCat.DataBindEx.Setters
{
    using Slash.Unity.DataBind.Foundation.Setters;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    [AddComponentMenu("HisaCat/Data Bind Extension/UnityUI/Setters/[DB] EnumString Group Active Setter (Unity)")]
    public class EnumStringGroupActiveSetter : EnumGroupActiveSetter<System.Enum>
    {
        public List<EnumStringGameObjectPair> EnumGameObjectMapping;

        /// <inheritdoc />
        protected override GameObject GetGameObject(System.Enum enumValue)
        {
            var enumGameObjectPair = this.EnumGameObjectMapping.FirstOrDefault(pair => pair.EnumStringValue == enumValue.ToString());
            return enumGameObjectPair != null ? enumGameObjectPair.GameObject : null;
        }

        /// <inheritdoc />
        protected override IEnumerable<GameObject> GetGameObjects()
        {
            return this.EnumGameObjectMapping.Select(pair => pair.GameObject).ToList();
        }

        [System.Serializable]
        public class EnumStringGameObjectPair
        {
            public string EnumStringValue;

            public GameObject GameObject;
        }

        public class EnumStringAttribute : PropertyAttribute
        {
            public EnumStringAttribute() { }
        }
    }
}
