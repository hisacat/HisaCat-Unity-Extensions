namespace HisaCat.HUE.DataBindEx.Providers
{
    using Slash.Unity.DataBind.Foundation.Providers.Objects;

    using UnityEngine;

    [AddComponentMenu("HisaCat/Data Bind Extension/Providers/[DB] Is iOS Platform Provider (Unity)")]
    public class IsIOSPlatformProvider : ConstantObjectProvider<bool>
    {
        public override bool ConstantValue
        {
            get
            {
#if UNITY_IOS
                return true;
#else
                return false;
#endif
            }
        }
    }
}
