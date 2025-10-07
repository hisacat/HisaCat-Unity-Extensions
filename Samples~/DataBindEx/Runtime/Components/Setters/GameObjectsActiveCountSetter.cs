namespace HisaCat.HUE.DataBindEx.Setters
{
    using Slash.Unity.DataBind.Foundation.Setters;
    using System.Collections.Generic;
    using UnityEngine;

    [AddComponentMenu("HisaCat/Data Bind Extension/UnityUI/Setters/[DB] Game Objects Active Count Setter (Unity)")]
    public class GameObjectsActiveCountSetter : SingleSetter<int>
    {
        /// <summary>
        ///     Game objects to enable/disable
        /// </summary>
        [Tooltip("Game objects to enable/disable")]
        public List<GameObject> GameObjects;

        /// <inheritdoc />
        public override void Disable()
        {
            base.Disable();
        }

        /// <inheritdoc />
        public override void Enable()
        {
            base.Enable();
        }

        /// <inheritdoc />
        protected override void OnValueChanged(int newValue)
        {
            for (int i = 0; i < this.GameObjects.Count; i++)
            {
                var gameObject = this.GameObjects[i];
                var activate = i < newValue;
                if (gameObject.activeSelf != activate)
                    gameObject.SetActive(activate);
            }
        }
    }
}
