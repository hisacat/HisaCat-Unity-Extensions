#if UNITY_EDITOR
using HisaCat.UnityExtensions;
using HisaCat.UnityExtensions.Editors;
using UnityEditor;
using UnityEngine;

namespace HisaCat
{
    public class SimpleCryptoWindow : EditorWindow
    {
        [MenuItem("HisaCat/SimpleCrypto/Open SimpleCrypto Window")]
        private static void Init()
        {
            SimpleCryptoWindow window = (SimpleCryptoWindow)EditorWindow.GetWindow(typeof(SimpleCryptoWindow));
            window.titleContent = new GUIContent("SimpleCrypto");
            window.Show();
        }

        private Vector2 scrollPos = Vector2.zero;

        private string textToEncrypt = string.Empty;
        private string secretkeyForEncrypt = string.Empty;
        private string encryptedText = string.Empty;

        private string textToDecrypt = string.Empty;
        private string secretkeyForDecrypt = string.Empty;
        private string decryptedText = string.Empty;

        private string textToHash = string.Empty;
        private string hashText = string.Empty;

        private string randCharWhitelist = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789`~!@#$%^&*()-_=+.,;";
        private int randStringLength = 16;
        private string randString = string.Empty;

        private string savSysPK = string.Empty;
        private string savSysPKH = string.Empty;

        private void OnGUI()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("SimpleCrypto", EditorStyles.largeLabel);
            EditorGUILayout.Space();

            this.scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            {
                GUILayout.Label("Encrypt");
                this.textToEncrypt = EditorGUILayout.TextField("Text", this.textToEncrypt);
                this.secretkeyForEncrypt = EditorGUILayout.TextField("Secret key", this.secretkeyForEncrypt);
                if (GUILayout.Button("Encrypt SHA128"))
                {
                    this.encryptedText = string.Empty;
                    this.encryptedText = SimpleCrypto.AES.AES128Encrypt(this.textToEncrypt, this.secretkeyForEncrypt);
                }
                EditorGUILayoutExtensions.ReadOnlyTextField("Encrypted", encryptedText);

                GUILayout.Space(EditorGUIUtility.singleLineHeight);

                GUILayout.Label("Decrypt");
                this.textToDecrypt = EditorGUILayout.TextField("Text", this.textToDecrypt);
                this.secretkeyForDecrypt = EditorGUILayout.TextField("Secret key", this.secretkeyForDecrypt);
                if (GUILayout.Button("Decrypt SHA128"))
                {
                    this.decryptedText = string.Empty;
                    if (SimpleCrypto.AES.AES128Decrypt(this.textToDecrypt, this.secretkeyForDecrypt, out var decrypted))
                        this.decryptedText = decrypted;
                    else
                        EditorUtility.DisplayDialog("SimpleCrypto", "Key wrong!", "Ok");
                }
                EditorGUILayoutExtensions.ReadOnlyTextField("Decrypted", decryptedText);

                GUILayout.Space(EditorGUIUtility.singleLineHeight);

                GUILayout.Label("Hash");
                this.textToHash = EditorGUILayout.TextField("Text", this.textToHash);
                if (GUILayout.Button("Get Hash String"))
                {
                    this.hashText = SimpleCrypto.Hash.GetHashString(this.textToHash);
                }
                EditorGUILayoutExtensions.ReadOnlyTextField("Hash", hashText);

                GUILayout.Space(EditorGUIUtility.singleLineHeight);
                GUILayout.Label("Random String");
                this.randCharWhitelist = EditorGUILayout.TextField("Characters", this.randCharWhitelist);
                this.randStringLength = EditorGUILayout.IntField("Length", this.randStringLength);
                if (GUILayout.Button("Create Random String"))
                {
                    var result = "";
                    for (int i = 0; i < this.randStringLength; i++)
                        result += this.randCharWhitelist.ToCharArray().PickRandom();
                    this.randString = result;
                }
                EditorGUILayoutExtensions.ReadOnlyTextField("Random String", this.randString);

                //GUILayout.Space(EditorGUIUtility.singleLineHeight);
                //GUILayout.Label("SaveSystem Key");
                //if (GUILayout.Button("Create New Key"))
                //{
                //    var res = SVV.SVVSaveSystem.CreateRandomKey();
                //    this.savSysPK = res.pk;
                //    this.savSysPKH = res.pkh;
                //}
                //EditorGUILayoutExtensions.ReadOnlyTextField("PK", this.savSysPK);
                //EditorGUILayoutExtensions.ReadOnlyTextField("PK Hash", this.savSysPKH);
            }
            EditorGUILayout.EndScrollView();
        }

        private void SetIndent(int amount, System.Action draw)
        {
            EditorGUI.indentLevel += amount;
            draw.Invoke();
            EditorGUI.indentLevel -= amount;
        }
    }
}
#endif
