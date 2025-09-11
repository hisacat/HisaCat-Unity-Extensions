using HisaCat.UnityExtensions;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace HisaCat
{
    public static class CryptoPlayerPrefs
    {
#if UNITY_EDITOR
        [UnityEditor.MenuItem("HisaCat/CryptoPlayerPrefs/Print Random Keys 16")]
        private static void PrintRandomKeys16()
        {
            var keys = string.Empty;
            for (int i = 0; i < 16; i++) keys += $"{RandomExtensions.GenerateRandomASCIIString(16)}\r\n";
            Debug.Log($"<b>[{nameof(CryptoPlayerPrefs)}]</b> {nameof(PrintRandomKeys16)}: {keys}");
        }
        [UnityEditor.MenuItem("HisaCat/CryptoPlayerPrefs/Print Hash Salt")]
        private static void PrintHashSalt()
        {
            Debug.Log($"<b>[{nameof(CryptoPlayerPrefs)}]</b> {nameof(PrintHashSalt)}: {HASH_SALT}");
        }
        [UnityEditor.MenuItem("HisaCat/CryptoPlayerPrefs/Print Crypto Key")]
        private static void PrintCryptoKey()
        {
            Debug.Log($"<b>[{nameof(CryptoPlayerPrefs)}]</b> {nameof(PrintCryptoKey)}: {CRYPTO_KEY}");
        }
#endif

        // Uses fixed random strings of length 16.
        // A new value can be generated for each project.
        // However, if the value changes within the same project, 
        // the new build will not be able to read values stored from the previous build.
        // Generate random string with: https://codebeautify.org/generate-random-string
        private const string HASH_SALT = "Qr|@MPWXyYO}KNZz";
        private const string CRYPTO_KEY = "Mmb~MuskWPpfc|vS";

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetInt(string key, int value) => SetString(key, value.ToString());
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetInt(string key) => GetInt(key, default);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetInt(string key, int defaultValue)
        {
            var rawValue = GetString(key, defaultValue.ToString());
            return int.TryParse(rawValue, out int result) ? result : defaultValue;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetFloat(string key, float value) => SetString(key, value.ToString());
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float GetFloat(string key) => GetFloat(key, default);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float GetFloat(string key, float defaultValue)
        {
            var rawValue = GetString(key, defaultValue.ToString());
            return float.TryParse(rawValue, out float result) ? result : defaultValue;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetBool(string key, bool value) => SetString(key, value.ToString());
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool GetBool(string key) => GetBool(key, default);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool GetBool(string key, bool defaultValue)
        {
            var rawValue = GetString(key, defaultValue.ToString());
            return bool.TryParse(rawValue, out bool result) ? result : defaultValue;
        }

        #region Core
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string CreateHashKey(string key)
        {
            return SimpleCrypto.Hash.GetHashString(key + HASH_SALT);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetString(string key, string value)
        {
            PlayerPrefs.SetString(CreateHashKey(key), SimpleCrypto.AES.AES128Encrypt(value, CRYPTO_KEY));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetString(string key) => GetString(key, string.Empty);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetString(string key, string defaultValue)
        {
            var encrypted = PlayerPrefs.GetString(CreateHashKey(key));
            if (string.IsNullOrEmpty(encrypted)) return defaultValue;

            if (SimpleCrypto.AES.AES128Decrypt(encrypted, CRYPTO_KEY, out string decrypted))
                return decrypted;
            else
                return defaultValue;
        }
        #endregion

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DeleteKey(string key) => PlayerPrefs.DeleteKey(CreateHashKey(key));
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasKey(string key) => PlayerPrefs.HasKey(CreateHashKey(key));
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Save() => PlayerPrefs.Save();
    }
}
