using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace HisaCat
{
    public static class SimpleCrypto
    {
        /// <summary>
        /// Fast but not secure. for defence memory edit.
        /// </summary>
        public static class BitShift
        {
#if UNITY_EDITOR
            [UnityEditor.MenuItem("HisaCat/SimpleCrypto/BitShift/PrintDefaultKey")]
            private static void PrintDefaultKey()
            {
                Debug.Log($"<b>[{nameof(SimpleCrypto)}]</b> {nameof(BitShift)} Current {nameof(DefaultKey)}: {DefaultKey}");
            }
#endif

            private static string _DefaultKey = null;
            private static string DefaultKey => _DefaultKey ??= Hash.GetHashString(System.Guid.NewGuid().ToString());

            public static string EncryptDycrypt(string value, int keyStartIdx = 0) => EncryptDycrypt(value, DefaultKey, keyStartIdx);
            public static string EncryptDycrypt(string value, string key, int keyStartIdx = 0)
            {
                if (value == null) return null;
                string result = "";
                for (int i = 0; i < value.Length; i++)
                    result += (char)(value[i] ^ key[(keyStartIdx + i) % key.Length]);
                return result;
            }
            public static byte[] EncryptDycrypt(byte[] value, int keyStartIdx = 0) => EncryptDycrypt(value, DefaultKey, keyStartIdx);
            public static byte[] EncryptDycrypt(byte[] value, string key, int keyStartIdx = 0)
            {
                if (value == null) return null;
                var result = new byte[value.Length];
                for (int i = 0; i < value.Length; i++)
                    result[i] = (byte)(value[i] ^ key[(keyStartIdx + i) % key.Length]);
                return result;
            }
        }

        /// <summary>
        /// Fast but not secure. for defence memory edit. Using BitShift and ASLR
        /// </summary>
        public static class FastCryptoField
        {
            public abstract class FieldBase<T>
            {
                private readonly string Key = "";
                private int keyStartIdx = 0;

                private byte[] rawValue = null;
                public T Value
                {
                    get => GetValueFromRawValue();
                    set => SetValueAsRawValue(value);
                }
                public FieldBase(T value)
                {
                    this.Key = System.Guid.NewGuid().ToString();
                    this.Value = value;
                }
                public FieldBase()
                {
                    this.Key = System.Guid.NewGuid().ToString();
                    this.Value = default;
                }

                protected T GetValueFromRawValue()
                {
                    var value = BitShift.EncryptDycrypt(this.rawValue, this.Key, keyStartIdx);
                    var result = TryParse(value);
                    return result;
                }
                protected void SetValueAsRawValue(T value)
                {
                    this.keyStartIdx = UnityEngine.Random.Range(0, this.Key.Length);
                    this.rawValue = BitShift.EncryptDycrypt(value != null ? ToRawValue(value) : null, this.Key, keyStartIdx);
                }
                protected abstract T TryParse(byte[] bytes);
                protected abstract byte[] ToRawValue(T value);
            }
            public class IntField : FieldBase<int>
            {
                public IntField() : base() { }
                public IntField(int defaultValue) : base(defaultValue) { }
                protected override int TryParse(byte[] bytes) => BitConverter.ToInt32(bytes);
                protected override byte[] ToRawValue(int value) => BitConverter.GetBytes(value);
            }
            public class FloatField : FieldBase<float>
            {
                public FloatField() : base() { }
                public FloatField(float defaultValue) : base(defaultValue) { }
                protected override float TryParse(byte[] bytes) => BitConverter.ToSingle(bytes);
                protected override byte[] ToRawValue(float value) => BitConverter.GetBytes(value);
            }
            public class DoubleField : FieldBase<double>
            {
                public DoubleField() : base() { }
                public DoubleField(double defaultValue) : base(defaultValue) { }
                protected override double TryParse(byte[] bytes) => BitConverter.ToDouble(bytes);
                protected override byte[] ToRawValue(double value) => BitConverter.GetBytes(value);
            }
            public class BoolField : FieldBase<bool>
            {
                public BoolField() : base() { }
                public BoolField(bool defaultValue) : base(defaultValue) { }
                protected override bool TryParse(byte[] bytes) => BitConverter.ToBoolean(bytes);
                protected override byte[] ToRawValue(bool value) => BitConverter.GetBytes(value);
            }
            public class StringField
            {
                private readonly string Key = "";
                private int keyStartIdx = 0;

                private string rawValue = null;
                public string Value
                {
                    get => BitShift.EncryptDycrypt(this.rawValue, this.Key, this.keyStartIdx);
                    set => this.rawValue = BitShift.EncryptDycrypt(value, this.Key, this.keyStartIdx = UnityEngine.Random.Range(0, this.Key.Length));
                }
                public StringField(string value)
                {
                    this.Key = System.Guid.NewGuid().ToString();
                    this.Value = value;
                }
                public StringField()
                {
                    this.Key = System.Guid.NewGuid().ToString();
                    this.Value = default;
                }
            }
        }

        public static class AES
        {
            private const string AES128_IV = "ZxqDo=ELkW_P$RHQ"; //Length: 16
            public static string AES128Encrypt(string Input, string key)
            {
                if (key.Length != 16)
                {
                    ManagedDebug.LogWarning("[SimpleCrypto.AES] AES128Encrypt: Key length is not 16! key will replaced front 16 characters of hash key");
                    key = Hash.GetHashString(key).Substring(0, 16);
                }

                using var aes = new RijndaelManaged
                {
                    KeySize = 128,
                    BlockSize = 128,
                    Mode = CipherMode.CBC,
                    Padding = PaddingMode.PKCS7,
                    Key = Encoding.UTF8.GetBytes(key),
                    IV = Encoding.UTF8.GetBytes(AES128_IV)
                };

                var encrypt = aes.CreateEncryptor(aes.Key, aes.IV);
                byte[] xBuff = null;
                using (var ms = new MemoryStream())
                {
                    using (var cs = new CryptoStream(ms, encrypt, CryptoStreamMode.Write))
                    {
                        byte[] xXml = Encoding.UTF8.GetBytes(Input);
                        cs.Write(xXml, 0, xXml.Length);
                    }

                    xBuff = ms.ToArray();
                }

                var result = Convert.ToBase64String(xBuff);
                return result;
            }
            public static bool AES128Decrypt(string Input, string key, out string decrypted)
            {
                if (key.Length != 16)
                {
                    ManagedDebug.LogWarning("[SimpleCrypto.AES] AES128Decrypt: Key length is not 16! key will replaced front 16 characters of hash key");
                    key = Hash.GetHashString(key).Substring(0, 16);
                }

                using var aes = new RijndaelManaged
                {
                    KeySize = 128,
                    BlockSize = 128,
                    Mode = CipherMode.CBC,
                    Padding = PaddingMode.PKCS7,
                    Key = Encoding.UTF8.GetBytes(key),
                    IV = Encoding.UTF8.GetBytes(AES128_IV)
                };

                var decrypt = aes.CreateDecryptor();
                byte[] xBuff = null;
                using (var ms = new MemoryStream())
                {
                    try
                    {
                        using (var cs = new CryptoStream(ms, decrypt, CryptoStreamMode.Write))
                        {
                            byte[] xXml = Convert.FromBase64String(Input);
                            cs.Write(xXml, 0, xXml.Length);
                        }
                    }
                    catch (Exception e)
                    {
                        ManagedDebug.LogError($"[SimpleCrypto.AES] AES128Decrypt: Failed to decrypt {e}");
                        decrypted = null;
                        return false;
                    }

                    xBuff = ms.ToArray();
                }

                decrypted = Encoding.UTF8.GetString(xBuff);
                return true;
            }
        }

        public static class Hash
        {
            private static HashAlgorithm _sha256HashAlgorithm = null;
            private static HashAlgorithm sha256HashAlgorithm => _sha256HashAlgorithm ??= SHA256.Create();

            private static StringBuilder _cachedStringBuilder = null;
            private static StringBuilder cachedStringBuilder = _cachedStringBuilder ??= new();


            public static byte[] GetHash(string inputString)
            {
                return sha256HashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(inputString));
            }

            /// <summary>
            /// Returns Hash as 64 Characters
            /// </summary>
            /// <param name="inputString"></param>
            /// <returns></returns>
            public static string GetHashString(string inputString)
            {
                cachedStringBuilder.Clear();

                foreach (byte b in GetHash(inputString))
                    cachedStringBuilder.Append(b.ToString("X2"));

                return cachedStringBuilder.ToString();
            }
        }
    }
}
