using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

namespace HisaCat.HUE
{
    /// <summary> TextMesh Pro 텍스트를 레이아웃 기준으로 페이지 단위 분할하는 확장 메서드입니다. </summary>
    public static class TextMeshProExtensions
    {
        /// <summary>
        /// 페이지 분할 시 "어디에서 끊을지"의 우선순위를 제어합니다.
        /// </summary>
        public enum SplitBoundaryMode
        {
            /// <summary>
            /// 기본 동작: 우선 줄바꿈(<c>\n</c>) 위치로 스냅하고, 없으면 공백(<c>' '</c>)으로 스냅합니다.
            /// </summary>
            NewLineThenSpace = 0,
            /// <summary>
            /// 공백(<c>' '</c>) 위치에서만 끊습니다.(<c>\n</c>은 경계로 스냅하지 않습니다)
            /// </summary>
            SpaceOnly = 1,
            /// <summary>
            /// 줄바꿈(<c>\n</c>) 위치에서만 끊습니다. (공백은 경계로 스냅하지 않습니다)
            /// </summary>
            NewLineOnly = 2
        }

        /// <summary> <see cref="TMP_Text.GetPreferredValues(string, float, float)"/> 결과와 한도 크기를 비교할 때 허용하는 부동소수점 오차입니다. </summary>
        private const float PreferredSizeComparisonEpsilon = 0.01f;

        /// <summary>
        /// <paramref name="textMeshPro"/>의 RectTransform·margin·폰트·줄바꿈 설정을 기준으로,
        /// overflow 없이 표시 가능한 크기로 <paramref name="text"/>를 페이지 단위로 잘라 반환합니다.
        /// </summary>
        /// <param name="textMeshPro">측정에 사용할 TMP 컴포넌트. 인스펙터에 설정된 스타일이 그대로 반영됩니다.</param>
        /// <param name="text">분할할 원본 문자열. TMP 리치 텍스트 태그를 포함할 수 있습니다.</param>
        /// <returns>
        /// 화면에 순서대로 넣을 수 있는 페이지 텍스트 목록입니다.
        /// 빈 문자열이면 빈 목록을 반환합니다.
        /// </returns>
        /// <remarks>
        /// <para>
        /// 가용 영역은 <c>rectTransform.rect</c>에서 TMP <see cref="TMP_Text.margin"/>(left, top, right, bottom)을 뺀 값입니다.
        /// </para>
        /// <para>
        /// Layout Group 등으로 Rect 크기가 아직 0이면 분할 결과를 신뢰할 수 없습니다.
        /// 호출 전 <c>Canvas.ForceUpdateCanvases()</c>로 레이아웃을 갱신하는 것을 권장합니다.
        /// </para>
        /// <para>
        /// 리치 텍스트(&lt;color&gt;, &lt;size&gt; 등)는 태그 중간이 아닌 경계에서만 자르며,
        /// 조각 끝에서는 열린 태그를 닫고 다음 조각 앞에서 동일 스타일 태그를 다시 엽니다.
        /// </para>
        /// <para>
        /// TMP가 지원하지 않는 커스텀 태그·잘못된 마크업은 파서가 예측하지 못할 수 있습니다.
        /// </para>
        /// </remarks>
        public static List<string> SplitTextIntoPages(TMP_Text textMeshPro, string text, SplitBoundaryMode boundaryMode = SplitBoundaryMode.NewLineThenSpace)
        {
            if (textMeshPro == null) throw new ArgumentNullException(nameof(textMeshPro));
            if (string.IsNullOrEmpty(text)) return new List<string>();

            var textRect = textMeshPro.rectTransform.rect;
            // margin: (left, top, right, bottom)
            var maxPageWidth = Mathf.Max(0f, textRect.width - textMeshPro.margin.x - textMeshPro.margin.z);
            var maxPageHeight = Mathf.Max(0f, textRect.height - textMeshPro.margin.y - textMeshPro.margin.w);
            return SplitTextIntoPages(textMeshPro, text, boundaryMode, maxPageWidth, maxPageHeight);
        }

        /// <summary>
        /// 지정한 가로·세로 한도(픽셀) 안에 overflow 없이 들어가도록 <paramref name="text"/>를 분할합니다.
        /// </summary>
        /// <param name="textMeshPro">측정에 사용할 TMP 컴포넌트.</param>
        /// <param name="text">분할할 원본 문자열.</param>
        /// <param name="maxPageWidth">한 페이지가 차지할 수 있는 최대 가로 크기(픽셀).</param>
        /// <param name="maxPageHeight">한 페이지가 차지할 수 있는 최대 세로 크기(픽셀).</param>
        /// <param name="boundaryMode">
        /// 페이지를 자를 때 우선적으로 맞출 경계 유형입니다.
        /// <list type="bullet">
        /// <item><description><see cref="SplitBoundaryMode.SpaceOnly"/>: 공백에서만 끊음</description></item>
        /// <item><description><see cref="SplitBoundaryMode.NewLineThenSpace"/>: 줄바꿈 우선, 없으면 공백</description></item>
        /// <item><description><see cref="SplitBoundaryMode.NewLineOnly"/>: 줄바꿈에서만 끊음</description></item>
        /// </list>
        /// 공백·줄바꿈이 전혀 없는 경우에는 <paramref name="boundaryMode"/>와 무관하게 글자 기준으로 잘립니다.
        /// </param>
        private static List<string> SplitTextIntoPages(
            TMP_Text textMeshPro,
            string text,
            SplitBoundaryMode boundaryMode,
            float maxPageWidth,
            float maxPageHeight)
        {
            if (textMeshPro == null) throw new ArgumentNullException(nameof(textMeshPro));
            if (string.IsNullOrEmpty(text)) return new List<string>();

            if (maxPageWidth <= PreferredSizeComparisonEpsilon
                || maxPageHeight <= PreferredSizeComparisonEpsilon)
                return new List<string> { text };

            var safeSliceEndIndices = RichTextSliceHelper.CollectRichTextSafeSplitIndices(text);
            var pages = new List<string>();
            var sliceStartIndex = 0;
            IReadOnlyList<string> inheritedOpenTags = Array.Empty<string>();

            while (sliceStartIndex < text.Length)
            {
                sliceStartIndex = RichTextSliceHelper.AdvanceToSafeSliceStartIndex(text, sliceStartIndex, safeSliceEndIndices);
                if (sliceStartIndex >= text.Length)
                    break;

                var sliceEndExclusiveIndex = FindMaxFittingEndIndex(
                    textMeshPro,
                    text,
                    sliceStartIndex,
                    inheritedOpenTags,
                    safeSliceEndIndices,
                    maxPageWidth,
                    maxPageHeight);

                sliceEndExclusiveIndex = AdjustSliceEndToWordBoundary(
                    text,
                    sliceStartIndex,
                    sliceEndExclusiveIndex,
                    safeSliceEndIndices,
                    boundaryMode);

                pages.Add(RichTextSliceHelper.BuildPageText(
                    text,
                    sliceStartIndex,
                    sliceEndExclusiveIndex,
                    inheritedOpenTags));

                inheritedOpenTags = RichTextSliceHelper.GetActiveOpenTagsBeforeIndex(text, sliceEndExclusiveIndex);
                sliceStartIndex = sliceEndExclusiveIndex;
            }

            return pages;
        }

        private static int FindMaxFittingEndIndex(
            TMP_Text textMeshPro,
            string text,
            int sliceStartIndex,
            IReadOnlyList<string> inheritedOpenTags,
            List<int> safeSliceEndIndices,
            float maxPageWidth,
            float maxPageHeight)
        {
            var candidateStart = safeSliceEndIndices.BinarySearch(sliceStartIndex + 1);
            if (candidateStart < 0)
                candidateStart = ~candidateStart;

            var candidateEnd = safeSliceEndIndices.Count - 1;
            if (candidateStart > candidateEnd)
                return Mathf.Min(sliceStartIndex + 1, text.Length);

            var searchRangeLow = candidateStart;
            var searchRangeHigh = candidateEnd;
            var bestEndExclusiveIndex = sliceStartIndex;

            while (searchRangeLow <= searchRangeHigh)
            {
                var candidateIndex = (searchRangeLow + searchRangeHigh) >> 1;
                var candidateEndExclusiveIndex = safeSliceEndIndices[candidateIndex];

                if (DoesPageFitInBounds(
                        textMeshPro,
                        text,
                        sliceStartIndex,
                        candidateEndExclusiveIndex,
                        inheritedOpenTags,
                        maxPageWidth,
                        maxPageHeight))
                {
                    bestEndExclusiveIndex = candidateEndExclusiveIndex;
                    searchRangeLow = candidateIndex + 1;
                }
                else
                {
                    searchRangeHigh = candidateIndex - 1;
                }
            }

            if (bestEndExclusiveIndex > sliceStartIndex)
                return bestEndExclusiveIndex;

            return safeSliceEndIndices[candidateStart];
        }

        private static bool DoesPageFitInBounds(
            TMP_Text textMeshPro,
            string text,
            int sliceStartIndex,
            int sliceEndExclusiveIndex,
            IReadOnlyList<string> inheritedOpenTags,
            float maxPageWidth,
            float maxPageHeight)
        {
            if (sliceEndExclusiveIndex <= sliceStartIndex)
                return false;

            var pageText = RichTextSliceHelper.BuildPageText(
                text,
                sliceStartIndex,
                sliceEndExclusiveIndex,
                inheritedOpenTags);

            var preferredSize = textMeshPro.GetPreferredValues(pageText, maxPageWidth, maxPageHeight);

            return preferredSize.x <= maxPageWidth + PreferredSizeComparisonEpsilon
                && preferredSize.y <= maxPageHeight + PreferredSizeComparisonEpsilon;
        }

        /// <summary>
        /// 최대 적합 길이를 <see cref="SplitBoundaryMode"/>에 맞는 공백·줄바꿈 경계로 되돌립니다.
        /// </summary>
        /// <remarks>
        /// 1) <c>FindMaxFittingEndIndex</c>로 페이지에 들어가는 최대 길이를 먼저 구합니다.<br/>
        /// 2) 그 위치가 이미 허용 경계(공백·줄바꿈)면 그대로 둡니다.<br/>
        /// 3) 아니면 모드에 따라 뒤쪽의 마지막 줄바꿈·공백으로 스냅합니다.<br/>
        /// 4) 범위 안에 줄바꿈·공백이 없으면 최대 길이 그대로 둡니다(단어 중간 분할).
        /// </remarks>
        private static int AdjustSliceEndToWordBoundary(
            string text,
            int sliceStartIndex,
            int sliceEndExclusiveIndex,
            List<int> safeSliceEndIndices,
            SplitBoundaryMode boundaryMode)
        {
            if (sliceEndExclusiveIndex <= sliceStartIndex + 1 || sliceEndExclusiveIndex >= text.Length)
                return sliceEndExclusiveIndex;

            if (IsAlreadyAtSplitBoundary(text, sliceEndExclusiveIndex, boundaryMode))
                return sliceEndExclusiveIndex;

            var searchEnd = sliceEndExclusiveIndex - 1;
            var searchStart = sliceStartIndex;
            var searchLength = searchEnd - searchStart + 1;

            if (boundaryMode == SplitBoundaryMode.NewLineThenSpace
                || boundaryMode == SplitBoundaryMode.NewLineOnly)
            {
                var lastNewlineIndex = text.LastIndexOf('\n', searchEnd, searchLength);
                if (lastNewlineIndex >= sliceStartIndex)
                {
                    var newlineEndExclusive = lastNewlineIndex + 1;
                    var safeNewlineEnd = RichTextSliceHelper.FindNearestSafeEndAtOrBefore(
                        safeSliceEndIndices,
                        newlineEndExclusive);
                    if (safeNewlineEnd > sliceStartIndex)
                        return safeNewlineEnd;
                }
            }

            if (boundaryMode == SplitBoundaryMode.NewLineThenSpace
                || boundaryMode == SplitBoundaryMode.SpaceOnly)
            {
                var lastSpaceIndex = text.LastIndexOf(' ', searchEnd, searchLength);
                if (lastSpaceIndex > sliceStartIndex)
                {
                    var safeSpaceEnd = RichTextSliceHelper.FindNearestSafeEndAtOrBefore(
                        safeSliceEndIndices,
                        lastSpaceIndex);
                    if (safeSpaceEnd > sliceStartIndex)
                        return safeSpaceEnd;
                }
            }

            return sliceEndExclusiveIndex;
        }

        /// <summary>
        /// 분할 위치가 이미 공백·줄바꿈 경계인지 여부입니다.
        /// </summary>
        /// <remarks>
        /// <c>true</c>이면 뒤로 스냅하지 않고, <c>FindMaxFittingEndIndex</c>가 찾은 최대 길이를 유지합니다.<br/>
        /// 줄바꿈은 모든 모드에서 자연 경계로 취급합니다(다음 문자가 <c>\n</c>이거나 직전 문자가 <c>\n</c>).
        /// </remarks>
        private static bool IsAlreadyAtSplitBoundary(string text, int splitIndexExclusive, SplitBoundaryMode boundaryMode)
        {
            if (splitIndexExclusive <= 0)
                return false;

            if (splitIndexExclusive >= text.Length)
                return true;

            var nextChar = text[splitIndexExclusive];
            var previousChar = text[splitIndexExclusive - 1];

            if (nextChar == '\n' || previousChar == '\n')
                return true;

            if (boundaryMode == SplitBoundaryMode.NewLineThenSpace
                || boundaryMode == SplitBoundaryMode.SpaceOnly)
            {
                if (nextChar == ' ' || previousChar == ' ')
                    return true;
            }

            return false;
        }

        /// <summary>
        /// TMP 리치 텍스트 마크업을 고려한 분할·태그 스택 유틸리티입니다.
        /// </summary>
        private static class RichTextSliceHelper
        {
            public static List<int> CollectRichTextSafeSplitIndices(string text)
            {
                var safeEnds = new List<int> { 0 };
                if (string.IsNullOrEmpty(text))
                    return safeEnds;

                var isInsideTag = false;
                for (var characterIndex = 0; characterIndex < text.Length; characterIndex++)
                {
                    var character = text[characterIndex];

                    if (character == '<')
                    {
                        isInsideTag = true;
                        continue;
                    }

                    if (isInsideTag)
                    {
                        if (character == '>')
                        {
                            isInsideTag = false;
                            TryAddSafeEnd(safeEnds, characterIndex + 1);
                        }

                        continue;
                    }

                    TryAddSafeEnd(safeEnds, characterIndex + 1);
                }

                TryAddSafeEnd(safeEnds, text.Length);
                return safeEnds;
            }

            public static int AdvanceToSafeSliceStartIndex(
                string text,
                int sliceStartIndex,
                List<int> safeSliceEndIndices)
            {
                if (sliceStartIndex <= 0 || IsSafeSliceIndex(text, sliceStartIndex))
                    return sliceStartIndex;

                var nextIndex = safeSliceEndIndices.BinarySearch(sliceStartIndex);
                if (nextIndex < 0)
                    nextIndex = ~nextIndex;

                return nextIndex < safeSliceEndIndices.Count
                    ? safeSliceEndIndices[nextIndex]
                    : text.Length;
            }

            public static string BuildPageText(
                string text,
                int sliceStartIndex,
                int sliceEndExclusiveIndex,
                IReadOnlyList<string> inheritedOpenTags)
            {
                var pageTextBuilder = new StringBuilder();

                AppendOpeningTags(pageTextBuilder, inheritedOpenTags);
                pageTextBuilder.Append(text, sliceStartIndex, sliceEndExclusiveIndex - sliceStartIndex);

                var openTagsAtSliceEnd = GetActiveOpenTagsBeforeIndex(text, sliceEndExclusiveIndex);
                AppendClosingTags(pageTextBuilder, openTagsAtSliceEnd);

                return pageTextBuilder.ToString();
            }

            public static List<string> GetActiveOpenTagsBeforeIndex(string text, int exclusiveEndIndex)
            {
                var openTags = new List<string>();
                if (string.IsNullOrEmpty(text) || exclusiveEndIndex <= 0)
                    return openTags;

                var parseLength = Mathf.Clamp(exclusiveEndIndex, 0, text.Length);
                var characterIndex = 0;

                while (characterIndex < parseLength)
                {
                    if (text[characterIndex] != '<')
                    {
                        characterIndex++;
                        continue;
                    }

                    var tagEndIndex = text.IndexOf('>', characterIndex + 1);
                    if (tagEndIndex < 0 || tagEndIndex >= parseLength)
                        break;

                    var fullTag = text.Substring(characterIndex, tagEndIndex - characterIndex + 1);
                    ApplyTagToStack(openTags, fullTag);
                    characterIndex = tagEndIndex + 1;
                }

                return openTags;
            }

            public static int FindNearestSafeEndAtOrBefore(List<int> safeSliceEndIndices, int targetEndExclusiveIndex)
            {
                var index = safeSliceEndIndices.BinarySearch(targetEndExclusiveIndex);
                if (index >= 0)
                    return safeSliceEndIndices[index];

                index = ~index - 1;
                return index >= 0 ? safeSliceEndIndices[index] : 0;
            }

            private static void ApplyTagToStack(List<string> openTags, string fullTag)
            {
                if (string.IsNullOrEmpty(fullTag) || fullTag[0] != '<')
                    return;

                var innerTag = fullTag.Substring(1, fullTag.Length - 2).Trim();
                if (innerTag.Length == 0)
                    return;

                if (innerTag[0] == '/')
                {
                    var closingTagName = GetTagName(innerTag.Substring(1));
                    for (var tagIndex = openTags.Count - 1; tagIndex >= 0; tagIndex--)
                    {
                        if (GetTagName(openTags[tagIndex]) == closingTagName)
                        {
                            openTags.RemoveAt(tagIndex);
                            break;
                        }
                    }

                    return;
                }

                if (IsSelfClosingTag(innerTag))
                    return;

                openTags.Add(fullTag);
            }

            private static bool IsSelfClosingTag(string innerTag)
            {
                if (innerTag.EndsWith("/", StringComparison.Ordinal))
                    return true;

                switch (GetTagName(innerTag))
                {
                    case "br":
                    case "BR":
                    case "cr":
                    case "CR":
                    case "zwsp":
                    case "ZWSP":
                    case "nobr":
                    case "NOBR":
                    case "page":
                    case "PAGE":
                    case "sprite":
                    case "SPRITE":
                    case "angle":
                    case "ANGLE":
                    case "hash":
                    case "HASH":
                    case "space":
                    case "SPACE":
                    case "u":
                    case "U":
                        return true;
                    default:
                        return false;
                }
            }

            private static string GetTagName(string tagContentOrFullTag)
            {
                var content = tagContentOrFullTag.Trim();
                if (content.StartsWith("<", StringComparison.Ordinal))
                    content = content.Substring(1, content.Length - 2).Trim();

                content = content.TrimStart('/');
                var delimiterIndex = content.IndexOfAny(new[] { ' ', '=', '"' });
                return delimiterIndex < 0
                    ? content
                    : content.Substring(0, delimiterIndex);
            }

            private static void AppendOpeningTags(StringBuilder builder, IReadOnlyList<string> openTags)
            {
                for (var tagIndex = 0; tagIndex < openTags.Count; tagIndex++)
                    builder.Append(openTags[tagIndex]);
            }

            private static void AppendClosingTags(StringBuilder builder, IReadOnlyList<string> openTags)
            {
                for (var tagIndex = openTags.Count - 1; tagIndex >= 0; tagIndex--)
                {
                    builder.Append("</");
                    builder.Append(GetTagName(openTags[tagIndex]));
                    builder.Append('>');
                }
            }

            private static bool IsSafeSliceIndex(string text, int index)
            {
                if (index <= 0 || index >= text.Length)
                    return true;

                var isInsideTag = false;
                for (var characterIndex = 0; characterIndex < index; characterIndex++)
                {
                    if (text[characterIndex] == '<')
                        isInsideTag = true;
                    else if (text[characterIndex] == '>')
                        isInsideTag = false;
                }

                return isInsideTag == false;
            }

            private static void TryAddSafeEnd(List<int> safeEnds, int exclusiveEndIndex)
            {
                if (safeEnds.Count == 0 || safeEnds[safeEnds.Count - 1] != exclusiveEndIndex)
                    safeEnds.Add(exclusiveEndIndex);
            }
        }
    }
}
