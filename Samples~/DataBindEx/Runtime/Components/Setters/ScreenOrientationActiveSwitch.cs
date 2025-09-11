namespace HisaCat.HUE.DataBindEx.Setters
{
    using Slash.Unity.DataBind.Core.Presentation;
    using UnityEngine;

    [AddComponentMenu("HisaCat/Data Bind Extension/Setters/[DB] Screen Orientation Active Switch (Unity)")]
    public class ScreenOrientationActiveSwitch : DataBindingOperator
    {
        /*
        Unknown = 0,
        Portrait = 1,
        PortraitUpsideDown = 2,
        Landscape = 3,
        LandscapeLeft = 3,
        LandscapeRight = 4,
        AutoRotation = 5
         */

        [DataTypeHintExplicit(typeof(GameObject))]
        public DataBinding ActiveWhenWholePortrait;
        [DataTypeHintExplicit(typeof(GameObject))]
        public DataBinding ActiveWhenWholeLandscape;

        [DataTypeHintExplicit(typeof(ScreenOrientation))]
        public DataBinding Switch;

        public override void Init()
        {
            base.Init();

            this.AddBinding(this.Switch);
            this.AddBinding(this.ActiveWhenWholePortrait);
            this.AddBinding(this.ActiveWhenWholeLandscape);
        }

        public override void Deinit()
        {
            base.Deinit();

            this.RemoveBinding(this.Switch);
            this.RemoveBinding(this.ActiveWhenWholePortrait);
            this.RemoveBinding(this.ActiveWhenWholeLandscape);
        }

        protected override void OnBindingValuesChanged()
        {
            var switchValue = this.Switch.GetValue<ScreenOrientation>();

            var activeWhenWholePortrait = this.ActiveWhenWholePortrait.GetValue<GameObject>();
            if (activeWhenWholePortrait != null)
                activeWhenWholePortrait.SetActive(switchValue == ScreenOrientation.LandscapeLeft || switchValue == ScreenOrientation.LandscapeRight);

            var activeWhenWholeLandscape = this.ActiveWhenWholeLandscape.GetValue<GameObject>();
            if (activeWhenWholeLandscape != null)
                activeWhenWholeLandscape.SetActive(switchValue == ScreenOrientation.Portrait || switchValue == ScreenOrientation.PortraitUpsideDown);
        }
    }
}
