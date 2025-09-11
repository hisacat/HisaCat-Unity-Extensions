using System.Collections.Generic;
using MiniJSON;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace HisaCat
{
    public static class Information
    {
        private static bool assetDatabaseRefreshed = false;
        [InitializeOnLoadMethod]
        public static void InitializeOnLoad()
        {
            assetDatabaseRefreshed = false;
        }

        public const string PackageManifestGUID = "9a1dabbf651a5c644bd0a3c5992786c3";
        private static PackageManifest _packageManifest = null;
        public static bool GetPackageManifest(out PackageManifest packageManifest)
        {
            if (_packageManifest == null)
            {
                // This method can be called from InitializeOnLoadMethod (ScriptingDefines.cs)
                // When the package is first loaded, the package manifest may not be loaded yet
                // So we manually refresh the asset database once to ensure it's available
                if (assetDatabaseRefreshed == false)
                {
                    AssetDatabase.Refresh();
                    assetDatabaseRefreshed = true;
                }

                _packageManifest = AssetDatabase.LoadAssetByGUID<PackageManifest>(new GUID(PackageManifestGUID));
                if (_packageManifest == null) Debug.LogError("Package manifest not found");
            }
            packageManifest = _packageManifest;
            return _packageManifest != null;
        }
        public static bool GetPackageManifestJson(out Dictionary<string, object> json)
        {
            if (GetPackageManifest(out PackageManifest pm) == false)
            {
                json = null;
                return false;
            }

            json = Json.Deserialize(pm.text) as Dictionary<string, object>;
            return json != null;
        }

        public static string GetName()
        {
            if (GetPackageManifestJson(out Dictionary<string, object> json) == false)
                return string.Empty;

            return json["name"] as string;
        }
        public static string GetDisplayName()
        {
            if (GetPackageManifestJson(out Dictionary<string, object> json) == false)
                return string.Empty;

            return json["displayName"] as string;
        }
        public static string GetVersion()
        {
            if (GetPackageManifestJson(out Dictionary<string, object> json) == false)
                return string.Empty;

            return json["version"] as string;
        }
    }
}
