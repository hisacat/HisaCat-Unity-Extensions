namespace HisaCat.DataBindEx.Providers.Checks
{
    using Slash.Unity.DataBind.Core.Presentation;
    using UnityEngine;

    /// <summary>
    ///     Indicates if the data string is not null or empty.
    /// </summary>
    [AddComponentMenu("HisaCat/Data Bind Extension/Checks/[DB] Is String Set Check")]
    public class IsStringSetCheck : DataProvider
    {
        [Header("It returns true only string is not null or empty")]
        [DataTypeHintExplicit(typeof(string))]
        public DataBinding Data;

        public override object Value
        {
            get
            {
                return string.IsNullOrEmpty(this.Data.GetValue<string>()) == false;
            }
        }

        public override void Init()
        {
            this.AddBinding(this.Data);
        }
        protected override void UpdateValue()
        {
            this.OnValueChanged();
        }
    }
}
