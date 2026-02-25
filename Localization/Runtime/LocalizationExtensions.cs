using System.Runtime.CompilerServices;
using UnityEngine;

namespace HisaCat.HUE.Localization.Extensions
{
    public static class LocalizationExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string KeyToLocalized(this string key) => LocalizationManager.Load(key);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string KeyToLocalized(this string key, SystemLanguage language) => LocalizationManager.Load(language, key);
    }
}
