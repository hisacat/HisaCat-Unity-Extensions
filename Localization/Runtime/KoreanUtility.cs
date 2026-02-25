using System.Globalization;
using System.Text;

namespace HisaCat.HUE.Localization
{
    /// <summary>
    /// 더 자연스러운 한국어 표현을 위한 유틸리티 클래스입니다.
    /// 조사 자동 치환 등 한국어 전용 문자열 처리를 제공합니다.
    /// </summary>
    public static class KoreanUtility
    {
        public static class JosaHelper
        {
            private const string Marker_EulReul = "%을를%";
            private const string Marker_EunNeun = "%은는%";
            private const string Marker_Iga = "%이가%";

            /// <summary>
            /// 주어진 문자열에 포함된 한국어 조사 토큰을 적절한 조사로 치환합니다.<br/>
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
            /// ResolveJosaTokens("나...%은는% 사과...%이가% 좋아요")<br/>
            /// → "나...는 사과...가 좋아요"<br/>
            /// </summary>
            /// <param name="input">치환 대상 문자열</param>
            /// <returns>치환 결과 문자열</returns>
            public static string ResolveJosaTokens(string input)
            {
                if (string.IsNullOrEmpty(input)) return input;

                // 성능을 위해 토큰 존재 여부를 먼저 확인하여 조기 반환합니다.
                if (input.Contains('%') == false) return input;

                // 주의:
                // 이 함수는 조사 토큰 치환뿐 아니라 "\%" 이스케이프도 처리합니다.
                // 따라서 "조사 토큰이 하나도 없으면 return" 최적화를 넣으면
                // "\%은는\%" 같은 입력의 이스케이프 처리 동작이 깨질 수 있습니다.
                //
                // 예) "\%은는\%" -> "%은는%" (정상)
                // 아래 조건식을 사용하면 위 변환이 수행되지 않음.
                //
                // if (input.Contains(Marker_EulReul) == false &&
                //     input.Contains(Marker_EunNeun) == false &&
                //     input.Contains(Marker_Iga) == false)
                //     return input;

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

            /// <summary> 현재 위치에서 target 문자열이 정확히 매칭되는지 확인합니다. </summary>
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

            /// <summary> 공백/구두점/기호/서로게이트/제어/포맷/분리자 등 스킵 대상인지 여부. </summary>
            private static bool IsSkippable(UnicodeCategory cat)
            {
                return cat switch
                {
                    // 공백/분리자
                    UnicodeCategory.SpaceSeparator or UnicodeCategory.LineSeparator or UnicodeCategory.ParagraphSeparator or
                    // 구두점
                    UnicodeCategory.ConnectorPunctuation or UnicodeCategory.DashPunctuation or UnicodeCategory.OpenPunctuation or UnicodeCategory.ClosePunctuation or
                    UnicodeCategory.InitialQuotePunctuation or UnicodeCategory.FinalQuotePunctuation or UnicodeCategory.OtherPunctuation or
                    // 기호(이모지 대부분 OtherSymbol로 분류)
                    UnicodeCategory.MathSymbol or UnicodeCategory.CurrencySymbol or UnicodeCategory.ModifierSymbol or UnicodeCategory.OtherSymbol or
                    // 서러게이트(이모지 조합 포함)
                    UnicodeCategory.Surrogate or
                    // 제어/포맷/결합표식 등은 기준 판단에 불필요
                    UnicodeCategory.Control or UnicodeCategory.Format or UnicodeCategory.NonSpacingMark or UnicodeCategory.SpacingCombiningMark or UnicodeCategory.EnclosingMark
                      => true,
                    _ => false,
                };
            }

            /// <summary> 한글 완성형(가~힣)인지 여부. </summary>
            private static bool IsHangulSyllable(char c) => c >= 0xAC00 && c <= 0xD7A3;

            /// <summary> 일반적인 문자/숫자(한글 제외)인지 여부. </summary>
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
