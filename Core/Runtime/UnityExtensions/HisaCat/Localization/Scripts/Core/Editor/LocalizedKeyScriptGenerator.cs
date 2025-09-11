using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace HisaCat.Localization
{
    public sealed class LocalizedKeyScriptGenerator : AssetPostprocessor
    {
        public static string GenerateLocalizedKeyClass(string json)
        {
            var dictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
            if (dictionary == null)
                throw new ArgumentException("Invalid JSON format.");

            var root = new Node("LocalizedKey"); // 루트 노드를 LocalizedKey로 설정

            foreach (var key in dictionary.Keys)
            {
                var parts = key.Split('.');
                root.Add(parts, key);
            }

            return $"namespace HisaCat.Localization\n{{\n{root.GenerateCode(1)}}}\n";
        }

        private class Node
        {
            public string Name { get; }
            public string Key { get; set; }
            public Dictionary<string, Node> Children { get; } = new();

            public Node(string name)
            {
                Name = name;
            }

            public void Add(string[] parts, string key)
            {
                if (parts.Length == 0) return;

                var part = parts[0];
                if (!Children.ContainsKey(part))
                {
                    Children[part] = new Node(part);
                }

                if (parts.Length == 1)
                {
                    Children[part].Key = key;
                }
                else
                {
                    Children[part].Add(parts.Skip(1).ToArray(), key);
                }
            }

            public string GenerateCode(int indent = 0)
            {
                var indentation = new string(' ', indent * 4);
                var code = "";

                if (!string.IsNullOrEmpty(Key))
                {
                    code += $"{indentation}public const string {NormalizeName(Name)} = \"{Key}\";\n";
                }
                else
                {
                    code += $"{indentation}public static class {NormalizeName(Name)}\n";
                    code += $"{indentation}{{\n";
                    foreach (var child in Children.Values)
                    {
                        code += child.GenerateCode(indent + 1);
                    }
                    code += $"{indentation}}}\n";
                }

                return code;
            }

            private string NormalizeName(string name)
            {
                var normalized = Regex.Replace(name, @"[^\w]", "_");
                if (char.IsDigit(normalized[0]))
                    normalized = "_" + normalized;
                return normalized;
            }
        }

        public const string ThisScriptPath = "Assets/HisaCat/Localization/Scripts/Core/Editor/LocalizedKeyScriptGenerator.cs";
        public const string TargetJsonPath = "Assets/Resources/Localization/ko_KR.json";
        public const string TargetScriptPath = "Assets/Localization/LocalizedKey.cs";
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            if (File.Exists(TargetJsonPath) == false)
            {
                Debug.Log($"[{nameof(LocalizedKeyScriptGenerator)}] Target json not found at \"{TargetJsonPath}\".");
                return;
            }

            var changedAssets = importedAssets.Concat(deletedAssets).Concat(movedAssets).Concat(movedFromAssetPaths);
            if (changedAssets.Contains(ThisScriptPath) || changedAssets.Contains(TargetJsonPath))
            {
                Debug.Log($"[{nameof(LocalizedKeyScriptGenerator)}] Target json detected from \"{TargetJsonPath}\".");
                var jsonAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(TargetJsonPath);
                var json = jsonAsset == null ? "{}" : jsonAsset.text;

                Debug.Log($"[{nameof(LocalizedKeyScriptGenerator)}] Generate...");
                var script = GenerateLocalizedKeyClass(json);


                Debug.Log($"[{nameof(LocalizedKeyScriptGenerator)}] Write script...");
                File.WriteAllText(TargetScriptPath, script);
                Debug.Log($"[{nameof(LocalizedKeyScriptGenerator)}] Script generated at \"{TargetScriptPath}\".");

                AssetDatabase.Refresh();
                Debug.Log($"[{nameof(LocalizedKeyScriptGenerator)}] AssetDatabase refreshed.");
            }
        }
    }
}
