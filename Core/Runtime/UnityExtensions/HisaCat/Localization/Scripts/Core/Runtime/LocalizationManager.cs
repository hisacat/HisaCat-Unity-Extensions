using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HisaCat.Localization
{
    /// <summary>
    /// NOTE: You can edit this for personal settings.
    /// </summary>
    public static class LocalizationSettings
    {
#if UNITY_EDITOR
#pragma warning disable IDE0051
        [UnityEditor.InitializeOnEnterPlayMode]
        private static void OnEnterPlaymodeInEditor(UnityEditor.EnterPlayModeOptions options)
        {
            if (options.HasFlag(UnityEditor.EnterPlayModeOptions.DisableDomainReload)) { }
        }
#pragma warning restore IDE0051
#endif

        public const string LocalizedJsonsPath = "Localization";
        public static readonly SystemLanguage DefaultLanguage = SystemLanguage.English;
        public static readonly SystemLanguage FallBackLanguage = SystemLanguage.English;
        public static readonly HashSet<SystemLanguage> SupportLanguages = new()
        {
            SystemLanguage.Afrikaans,
            SystemLanguage.Arabic,
            SystemLanguage.Belarusian,
            SystemLanguage.Bulgarian,
            SystemLanguage.Catalan,
            SystemLanguage.Czech,
            SystemLanguage.Danish,
            SystemLanguage.German,
            SystemLanguage.Greek,
            SystemLanguage.English,
            SystemLanguage.Spanish,
            SystemLanguage.Estonian,
            SystemLanguage.Basque,
            SystemLanguage.Finnish,
            SystemLanguage.Faroese,
            SystemLanguage.French,
            SystemLanguage.Hebrew,
            SystemLanguage.Hindi,
            SystemLanguage.Hungarian,
            SystemLanguage.Indonesian,
            SystemLanguage.Icelandic,
            SystemLanguage.Italian,
            SystemLanguage.Japanese,
            SystemLanguage.Korean,
            SystemLanguage.Lithuanian,
            SystemLanguage.Latvian,
            SystemLanguage.Dutch,
            SystemLanguage.Norwegian,
            SystemLanguage.Polish,
            SystemLanguage.Portuguese,
            SystemLanguage.Romanian,
            SystemLanguage.Russian,
            SystemLanguage.SerboCroatian,
            SystemLanguage.Slovak,
            SystemLanguage.Slovenian,
            SystemLanguage.Swedish,
            SystemLanguage.Thai,
            SystemLanguage.Turkish,
            SystemLanguage.Ukrainian,
            SystemLanguage.Vietnamese,
            SystemLanguage.ChineseSimplified,
            SystemLanguage.ChineseTraditional,
        };

        public const bool PrintMissingLanguageLogs = true;
        public const bool PrintMissingKeyLogs = true;

        public static readonly LocalizedTexts.LoadJsonEventHandler LoadJsonHandler = null;
        //Example of LoadJsonHandler
        //public static readonly LocalizedTexts.LoadJsonEventHandler LoadJsonHandler = (path, lang)=>
        //{
        //    var jsonPath = string.IsNullOrEmpty(path) ? LocalizedTexts.GetLocaleStr(lang) : $"{path}/{LocalizedTexts.GetLocaleStr(lang)}";
        //    var jsonAsset = Resources.Load<TextAsset>(jsonPath);
        //    return jsonAsset == null ? null : jsonAsset.text;
        //};

#if UNITY_EDITOR
        public const bool AutoUpdateLocalizedTextOnEditor = true;
#endif
    }

    public static class LocalizationManager
    {
        static LocalizationManager()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.playModeStateChanged += (state) =>
            {
                if (state == UnityEditor.PlayModeStateChange.ExitingPlayMode)
                    SelectedLanguage = LocalizationSettings.DefaultLanguage;
            };
#endif
        }

#if UNITY_EDITOR
#pragma warning disable IDE0051
        [UnityEditor.InitializeOnEnterPlayMode]
        private static void OnEnterPlaymodeInEditor(UnityEditor.EnterPlayModeOptions options)
        {
            if (options.HasFlag(UnityEditor.EnterPlayModeOptions.DisableDomainReload))
            {
                dic = null;
            }
        }
#pragma warning restore IDE0051
#endif

        #region Clear for editor preview
#if UNITY_EDITOR
        public class UpdateLocalizedTextPostprocessor : UnityEditor.AssetPostprocessor
        {
#if UNITY_EDITOR
#pragma warning disable IDE0051
            [UnityEditor.InitializeOnEnterPlayMode]
            private static void OnEnterPlaymodeInEditor(UnityEditor.EnterPlayModeOptions options)
            {
                if (options.HasFlag(UnityEditor.EnterPlayModeOptions.DisableDomainReload))
                {
                    _selectedLanguage = LocalizationSettings.DefaultLanguage;
                    OnLanguageChanged = null;
                    dic = null;
                }
            }
#pragma warning restore IDE0051
#endif

            private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
            {
                foreach (var asset in importedAssets)
                {
                    if (asset.EndsWith($"Resources/{LocalizationSettings.LocalizedJsonsPath}/{LocalizationSettings.DefaultLanguage.ToLocaleString()}.json", StringComparison.OrdinalIgnoreCase))
                    {
                        // Clear localized text dictionary when locale json was changed.
                        dic = null;

                        DoUpdateTextsOnEditor();
                        return;
                    }
                }
            }
        }

        private static void DoUpdateTextsOnEditor()
        {
            if (Application.isPlaying) return;

            // Force update localized texts.
            var texts = FindInterfacesOfType<IEditorLocalizedTextUpdateable>();
            foreach (var text in texts)
                text.UpdateTextOnEditor();

            static IEnumerable<T> FindInterfacesOfType<T>(bool includeInactive = false)
            {
                var scenes = new List<Scene>();
                for (int i = 0; i < SceneManager.sceneCount; i++)
                    scenes.Add(SceneManager.GetSceneAt(i));

                // Scene texts.
                var texts = scenes
                    .SelectMany(e =>
                        e.GetRootGameObjects().SelectMany(go =>
                            go.GetComponentsInChildren<T>(includeInactive)));

                // Prefab stage texts.
                var prefabStage = UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();
                if (prefabStage != null)
                    texts = texts.Concat(prefabStage.scene.GetRootGameObjects().SelectMany(go => go.GetComponentsInChildren<T>(includeInactive)));

                return texts;
            }
        }
#endif
        #endregion

        #region Language selection
        private static SystemLanguage _selectedLanguage = LocalizationSettings.DefaultLanguage;
        public static SystemLanguage SelectedLanguage
        {
            get => _selectedLanguage;
            set
            {
                if (LocalizationSettings.SupportLanguages.Contains(value) == false)
                {
                    Debug.LogError($"[{nameof(LocalizationManager)}] language \"{value}\" does not support! (Not exists in \"{nameof(LocalizationSettings.SupportLanguages)}\")");
                    return;
                }

                if (_selectedLanguage == value)
                    return;

                var prevLang = _selectedLanguage;
                _selectedLanguage = value;

#if UNITY_EDITOR
                DoUpdateTextsOnEditor();
#endif

                OnLanguageChangedCallback(prevLang, _selectedLanguage);
            }
        }
        public delegate void OnLanguageChangedEventHandler(SystemLanguage prevLang, SystemLanguage curLang);
        public static event OnLanguageChangedEventHandler OnLanguageChanged = null;
        private static void OnLanguageChangedCallback(SystemLanguage prevLang, SystemLanguage curLang)
        {
            OnLanguageChanged?.Invoke(prevLang, curLang);
        }
        #endregion

        private static Dictionary<string, LocalizedTexts> dic = null;

        public static bool Exists(string key) => Exists(LocalizationManager.SelectedLanguage, key);
        public static bool Exists(SystemLanguage lang, string key) => Exists(lang, LocalizationSettings.LocalizedJsonsPath, key);
        public static bool Exists(string path, string key) => Exists(LocalizationManager.SelectedLanguage, path, key);
        public static bool Exists(SystemLanguage lang, string path, string key)
        {
            if (dic == null)
                dic = new Dictionary<string, LocalizedTexts>(StringComparer.OrdinalIgnoreCase);

            if (dic.ContainsKey(path) == false)
                dic.Add(path, new LocalizedTexts(path, LocalizationSettings.LoadJsonHandler));

            return dic[path].Exists(lang, key);
        }

        public static string Load(string key) => Load(LocalizationManager.SelectedLanguage, key);
        public static string Load(SystemLanguage lang, string key) => Load(lang, LocalizationSettings.LocalizedJsonsPath, key);
        public static string Load(string path, string key) => Load(LocalizationManager.SelectedLanguage, path, key);
        public static string Load(SystemLanguage lang, string path, string key, SystemLanguage? fallback = null)
        {
            if (dic == null)
                dic = new Dictionary<string, LocalizedTexts>(StringComparer.OrdinalIgnoreCase);

            if (dic.ContainsKey(path) == false)
                dic.Add(path, new LocalizedTexts(path, LocalizationSettings.LoadJsonHandler));

            return dic[path].Load(lang, key, fallback == null ? LocalizationSettings.FallBackLanguage : fallback.Value);
        }
    }

    public class LocalizedTexts
    {
#if UNITY_EDITOR
        [UnityEditor.MenuItem("HisaCat/Localization/Create Localized String Templates")]
        public static void CreateTemplate()
        {
            var path = UnityEditor.EditorUtility.SaveFolderPanel("Path", "Assets", "Localized");
            if (string.IsNullOrEmpty(path))
                return;

            var data = new Dictionary<string, string>();
            for (int i = 0; i < 3; i++)
                data.Add($"key{i}", "text");

            var json = JsonConvert.SerializeObject(data, Formatting.Indented);
            var langs = new SystemLanguage[] { SystemLanguage.English, SystemLanguage.Korean, SystemLanguage.Japanese, SystemLanguage.ChineseSimplified, SystemLanguage.ChineseTraditional };
            foreach (var lang in langs)
            {
                var filePath = System.IO.Path.Combine(path, $"{lang.ToLocaleString()}.json");
                System.IO.File.WriteAllText(filePath, json);
                Debug.Log($"[{nameof(LocalizationManager)}] Template for ${lang} created at ${filePath}");
            }

            UnityEditor.EditorUtility.DisplayDialog("Localization", "Localized String Templates created and saved.", "Ok");
            UnityEditor.AssetDatabase.Refresh();
            var folderAssetPath = path;
            if (folderAssetPath.StartsWith(Application.dataPath))
                folderAssetPath = "Assets" + folderAssetPath.Substring(Application.dataPath.Length);
            var folderAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(folderAssetPath);
            UnityEditor.EditorGUIUtility.PingObject(folderAsset);
        }
#endif

        public delegate string LoadJsonEventHandler(string path, SystemLanguage lang);

        private readonly Dictionary<SystemLanguage, Dictionary<string, string>> Dic = null;
        private readonly LoadJsonEventHandler loadJsonHandler = null;
        public readonly string Path = string.Empty;
        public LocalizedTexts(string path, LoadJsonEventHandler loadJsonHandler = null)
        {
            this.Path = path;
            this.Dic = new Dictionary<SystemLanguage, Dictionary<string, string>>();
            this.loadJsonHandler = loadJsonHandler != null ? loadJsonHandler : DefaultLoadJsonEventHandler;
        }

        public string DefaultLoadJsonEventHandler(string path, SystemLanguage lang)
        {
            var jsonPath = string.IsNullOrEmpty(path) ? lang.ToLocaleString() : $"{path}/{lang.ToLocaleString()}";
            var jsonAsset = Resources.Load<TextAsset>(jsonPath);
            return jsonAsset == null ? null : jsonAsset.text;
        }

        private bool IsLanguageLoaded(SystemLanguage lang) => this.Dic.ContainsKey(lang);
        private void LoadLanguage(SystemLanguage lang)
        {
            if (IsLanguageLoaded(lang)) return;

            var json = this.loadJsonHandler(this.Path, lang);

            if (string.IsNullOrEmpty(json))
            {
                if (LocalizationSettings.PrintMissingLanguageLogs)
                    Debug.LogWarning($"[{nameof(LocalizationManager)}] {nameof(Load)}: Cannot find {lang} language.");
            }

            // Parse json.
            var keyTextPair = json == null ? new Dictionary<string, string>() : JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
            this.Dic.Add(lang, keyTextPair);
        }

        public bool Exists(SystemLanguage lang, string key)
        {
            if (key == null) return false;

            // Load if target language not exists in dictionary.
            if (IsLanguageLoaded(lang) == false) LoadLanguage(lang);

            return this.Dic[lang].ContainsKey(key);
        }

        public string Load(SystemLanguage lang, string key, SystemLanguage fallbackLang)
        {
            if (key == null) return null;

            var keyExists = Exists(lang, key);

            // [MEMO] "Exists" function already loads language if not exists.
            // // Load if target language not exists in dictionary.
            // if (IsLanguageLoaded(lang) == false) LoadLanguage(lang);

            // Fallback if key not exists in target language.
            if (keyExists == false)
            {
                if (LocalizationSettings.PrintMissingKeyLogs)
                    Debug.LogWarning($"[{nameof(LocalizationManager)}] {nameof(Load)}: Cannot find key \"{key}\" at {lang} language.");

                if (lang == fallbackLang) return key;

                return Load(fallbackLang, key, fallbackLang);
            }

            var value = this.Dic[lang][key];
            if (lang == SystemLanguage.Korean)
                value = KoreanHelper.JosaHelper.ReplaceJosa(value);

            return value;
        }
    }

    public static class KoreanHelper
    {
        public static class JosaHelper
        {
            private const string Marker_EulReul = "%을를%";
            private const string Marker_EunNeun = "%은는%";
            private const string Marker_Iga = "%이가%";

            /// <summary>
            /// 입력 문자열에서 한국어 조사를 자동 치환합니다.<br/>
            /// <br/>
            /// <b>대상 토큰</b><br/>
            /// %을를%  → 받침 O: "을", 받침 X: "를", 기준 문자 부재/비한글: "(을)를"<br/>
            /// %은는%  → 받침 O: "은", 받침 X: "는", 기준 문자 부재/비한글: "(은)는"<br/>
            /// %이가%  → 받침 O: "이", 받침 X: "가", 기준 문자 부재/비한글: "(이)가"<br/>
            /// <br/>
            /// <b>기준 문자 탐색 규칙</b><br/>
            /// - 토큰 직전에서 공백/구두점/기호/이모지(서로게이트) 등은 <i>건너뛰고</i> 검사합니다.<br/>
            /// - 한글 완성형(가~힣)을 만나면 종성 유무로 조사 선택을 결정합니다.<br/>
            /// - 영문/숫자/그 밖의 일반 문자(한글 아님)를 만나면 종성 판정 불가로 보고 <i>중립 표기</i>를 사용합니다.<br/>
            /// - 시작까지 가도 기준 문자를 못 찾으면 <i>중립 표기</i>를 사용합니다.<br/>
            /// <br/>
            /// <b>이스케이프</b><br/>
            /// \% → 리터럴 '%' 로 출력(백슬래시는 소비됨). 예: "\%을를\%" → "%을를%"<br/>
            /// <br/>
            /// <b>예시</b><br/>
            /// ReplaceJosa("나...%은는% 사과...%이가% 좋아요")<br/>
            /// → "나...는 사과...가 좋아요"<br/>
            /// </summary>
            /// <param name="input">치환 대상 문자열</param>
            /// <returns>치환 결과 문자열</returns>
            public static string ReplaceJosa(string input)
            {
                if (string.IsNullOrEmpty(input)) return input ?? string.Empty;

                var sb = new StringBuilder(input.Length);
                int i = 0;

                while (i < input.Length)
                {
                    char c = input[i];

                    // 1) 이스케이프: "\%" → '%' (백슬래시는 소비)
                    if (c == '\\')
                    {
                        if (i + 1 < input.Length && input[i + 1] == '%')
                        {
                            sb.Append('%');
                            i += 2;
                            continue;
                        }
                        sb.Append('\\');
                        i++;
                        continue;
                    }

                    // 2) 토큰 매칭
                    if (c == '%')
                    {
                        if (Matches(input, i, Marker_EulReul))
                        {
                            AppendResolved(sb, "을", "를", "(을)를");
                            i += Marker_EulReul.Length;
                            continue;
                        }
                        if (Matches(input, i, Marker_EunNeun))
                        {
                            AppendResolved(sb, "은", "는", "(은)는");
                            i += Marker_EunNeun.Length;
                            continue;
                        }
                        if (Matches(input, i, Marker_Iga))
                        {
                            AppendResolved(sb, "이", "가", "(이)가");
                            i += Marker_Iga.Length;
                            continue;
                        }

                        // 토큰이 아니면 리터럴 %
                        sb.Append('%');
                        i++;
                        continue;
                    }

                    // 3) 일반 문자 복사
                    sb.Append(c);
                    i++;
                }

                return sb.ToString();
            }

            /// <summary>
            /// 기준 문자(토큰 직전의 유효한 한글) 탐색 결과를 기반으로
            /// 받침O/받침X/중립 표기 중 하나를 Append 합니다.
            /// </summary>
            /// <param name="sb">출력 버퍼</param>
            /// <param name="jongCase">받침이 있을 때 사용할 조사</param>
            /// <param name="noJongCase">받침이 없을 때 사용할 조사</param>
            /// <param name="neutral">기준 문자 부재 또는 비한글일 때 사용할 중립 표기</param>
            private static void AppendResolved(StringBuilder sb, string jongCase, string noJongCase, string neutral)
            {
                var ctx = AnalyzePreviousContext(sb);

                if (ctx.Kind == PrevKind.Hangul)
                {
                    sb.Append(ctx.HasJong ? jongCase : noJongCase);
                }
                else
                {
                    sb.Append(neutral);
                }
            }

            /// <summary>
            /// 현재 위치에서 target 문자열이 정확히 매칭되는지 확인합니다.
            /// </summary>
            private static bool Matches(string s, int index, string target)
            {
                if (index + target.Length > s.Length) return false;
                for (int k = 0; k < target.Length; k++)
                {
                    if (s[index + k] != target[k]) return false;
                }
                return true;
            }

            /// <summary>
            /// 토큰 직전의 "유효한 기준 문자"를 찾기 위해 출력 버퍼를 역방향으로 스캔합니다.<br/>
            /// - 공백/구두점/기호/서로게이트/제어/포맷 문자는 건너뜁니다.<br/>
            /// - 한글 완성형(가~힣)을 만나면 종성 유무를 반환합니다.<br/>
            /// - 그 밖의 문자 범주(영문/숫자/기타 일반 문자)를 만나면 중립 판정으로 종료합니다.<br/>
            /// - 버퍼 시작까지 기준 문자를 못 찾으면 중립 판정입니다.
            /// </summary>
            private static PrevContext AnalyzePreviousContext(StringBuilder sb)
            {
                for (int idx = sb.Length - 1; idx >= 0; idx--)
                {
                    char ch = sb[idx];
                    UnicodeCategory cat = char.GetUnicodeCategory(ch);

                    // 공백/구두점/기호/서로게이트/제어/포맷 등 → 건너뜀
                    if (IsSkippable(cat))
                        continue;

                    // 한글 완성형이면 종성 판정
                    if (IsHangulSyllable(ch))
                        return new PrevContext(PrevKind.Hangul, HasJongseong(ch));

                    // 그 외의 일반 문자(영문/숫자/기타 Letter/Number 등) → 중립 처리
                    if (IsGeneralLetterOrNumber(cat))
                        return new PrevContext(PrevKind.NonHangul, false);

                    // 혹시 남는 경우가 있어도 안전하게 중립
                    return new PrevContext(PrevKind.NonHangul, false);
                }

                // 기준 없음 → 중립
                return new PrevContext(PrevKind.None, false);
            }

            /// <summary>
            /// 공백/구두점/기호/서로게이트/제어/포맷/분리자 등 스킵 대상인지 여부.
            /// </summary>
            private static bool IsSkippable(UnicodeCategory cat)
            {
                switch (cat)
                {
                    // 공백/분리자
                    case UnicodeCategory.SpaceSeparator:
                    case UnicodeCategory.LineSeparator:
                    case UnicodeCategory.ParagraphSeparator:
                    // 구두점
                    case UnicodeCategory.ConnectorPunctuation:
                    case UnicodeCategory.DashPunctuation:
                    case UnicodeCategory.OpenPunctuation:
                    case UnicodeCategory.ClosePunctuation:
                    case UnicodeCategory.InitialQuotePunctuation:
                    case UnicodeCategory.FinalQuotePunctuation:
                    case UnicodeCategory.OtherPunctuation:
                    // 기호(이모지 대부분 OtherSymbol로 분류)
                    case UnicodeCategory.MathSymbol:
                    case UnicodeCategory.CurrencySymbol:
                    case UnicodeCategory.ModifierSymbol:
                    case UnicodeCategory.OtherSymbol:
                    // 서러게이트(이모지 조합 포함)
                    case UnicodeCategory.Surrogate:
                    // 제어/포맷/결합표식 등은 기준 판단에 불필요
                    case UnicodeCategory.Control:
                    case UnicodeCategory.Format:
                    case UnicodeCategory.NonSpacingMark:
                    case UnicodeCategory.SpacingCombiningMark:
                    case UnicodeCategory.EnclosingMark:
                        return true;
                    default:
                        return false;
                }
            }

            /// <summary>
            /// 한글 완성형(가~힣)인지 여부.
            /// </summary>
            private static bool IsHangulSyllable(char c)
                => c >= 0xAC00 && c <= 0xD7A3;

            /// <summary>
            /// 일반적인 문자/숫자(한글 제외)인지 여부.
            /// </summary>
            private static bool IsGeneralLetterOrNumber(UnicodeCategory cat)
            {
                switch (cat)
                {
                    case UnicodeCategory.UppercaseLetter:
                    case UnicodeCategory.LowercaseLetter:
                    case UnicodeCategory.TitlecaseLetter:
                    case UnicodeCategory.ModifierLetter:
                    case UnicodeCategory.OtherLetter:
                    case UnicodeCategory.DecimalDigitNumber:
                    case UnicodeCategory.LetterNumber:
                    case UnicodeCategory.OtherNumber:
                        return true;
                    default:
                        return false;
                }
            }

            /// <summary>
            /// 한글 완성형의 종성(받침) 존재 여부.<br/>
            /// (코드포인트 - 0xAC00) % 28 != 0  → 받침 있음
            /// </summary>
            private static bool HasJongseong(char c)
            {
                if (!IsHangulSyllable(c)) return false;
                int code = c - 0xAC00;
                int jong = code % 28;
                return jong != 0;
            }

            private enum PrevKind { None, Hangul, NonHangul }

            private readonly struct PrevContext
            {
                public PrevKind Kind { get; }
                public bool HasJong { get; }
                public PrevContext(PrevKind kind, bool hasJong)
                {
                    Kind = kind;
                    HasJong = hasJong;
                }
            }
        }
    }

}
