using System.Collections.Generic;
using UnityEngine;

namespace HisaCat.HUE.Settings
{
    internal class HueSettingsProvider : MonoBehaviour
    {
        internal const string RootPath = "Project/HisaCat/HUE";
        [UnityEditor.SettingsProvider]
        internal static UnityEditor.SettingsProvider CreateSettingsProvider()
        {
            // SettingsScope.Project: Project Settings
            // SettingsScope.Project: Preferences
            return new(RootPath, UnityEditor.SettingsScope.Project)
            {
                label = "HUE: HisaCat's Unity Extensions",
                guiHandler = (searchContext) =>
                {
                },
                keywords = new HashSet<string> { "HisaCat", "HUE" }
            };
        }
    }
}