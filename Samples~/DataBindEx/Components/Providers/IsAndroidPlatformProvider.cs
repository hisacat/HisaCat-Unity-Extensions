namespace HisaCat.DataBindEx.Providers
{
    using Slash.Unity.DataBind.Foundation.Providers.Objects;

    using UnityEngine;

    [AddComponentMenu("HisaCat/Data Bind Extension/Providers/[DB] Is Android Platform Provider (Unity)")]
    public class IsAndroidPlatformProvider : ConstantObjectProvider<bool>
    {
        public override bool ConstantValue
        {
            get
            {
#if UNITY_ANDROID
                return true;
#else
                return false;
#endif
            }
        }
    }
}
