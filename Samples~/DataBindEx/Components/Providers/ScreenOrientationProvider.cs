namespace HisaCat.DataBindEx.Providers
{
    using Slash.Unity.DataBind.Core.Presentation;

    using UnityEngine;

    [AddComponentMenu("HisaCat/Data Bind Extension/Providers/[DB] Screen Orientation Provider (Unity)")]
    public class ScreenOrientationProvider : DataProvider
    {
        private ScreenOrientation lastOrientation = default;

        public override object Value => Screen.orientation;

        private void Update()
        {
            if (this.lastOrientation != Screen.orientation)
            {
                this.lastOrientation = Screen.orientation;
                this.OnValueChanged();
            }
        }
    }
}
