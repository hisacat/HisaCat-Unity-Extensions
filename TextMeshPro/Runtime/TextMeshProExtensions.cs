using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace HisaCat.HUE
{
    /// <summary>
    /// TextMesh Pro 텍스트 측정·분할 관련 확장 메서드입니다.
    /// </summary>
    public static class TextMeshProExtensions
    {
        /// <summary>
        /// <see cref="TMP_Text.GetPreferredValues"/> 결과와 한도 크기를 비교할 때 허용하는 부동소수점 오차입니다.
        /// </summary>
        private const float PreferredSizeComparisonEpsilon = 0.01f;

        /// <summary>
        /// <paramref name="textMeshPro"/>의 RectTransform·margin·폰트·줄바꿈 설정을 기준으로,
        /// overflow 없이 표시 가능한 크기로 <paramref name="text"/>를 순서대로 잘라 반환합니다.
        /// </summary>
        /// <param name="textMeshPro">측정에 사용할 TMP 컴포넌트. 인스펙터에 설정된 스타일이 그대로 반영됩니다.</param>
        /// <param name="text">분할할 원본 문자열.</param>
        /// <returns>
        /// 화면에 순서대로 넣을 수 있는 문자열 조각 목록.
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
        /// 리치 텍스트(&lt;color&gt; 등)는 문자 인덱스 기준으로 자르므로 태그가 깨질 수 있습니다.
        /// 태그가 포함된 문자열은 태그 단위 분할 등 별도 처리가 필요합니다.
        /// </para>
        /// </remarks>
        public static List<string> SplitTextByDisplayableLength(TMP_Text textMeshPro, string text)
        {
            if (textMeshPro == null) throw new ArgumentNullException(nameof(textMeshPro));
            if (string.IsNullOrEmpty(text)) return new List<string>();

            var textRect = textMeshPro.rectTransform.rect;
            // margin: (left, top, right, bottom)
            var displayableWidth = Mathf.Max(0f, textRect.width - textMeshPro.margin.x - textMeshPro.margin.z);
            var displayableHeight = Mathf.Max(0f, textRect.height - textMeshPro.margin.y - textMeshPro.margin.w);
            return SplitTextByDisplayableLength(textMeshPro, text, displayableWidth, displayableHeight);
        }

        /// <summary>
        /// 지정한 가로·세로 한도(픽셀) 안에 overflow 없이 들어가도록 <paramref name="text"/>를 분할합니다.
        /// </summary>
        /// <param name="textMeshPro">측정에 사용할 TMP 컴포넌트.</param>
        /// <param name="text">분할할 원본 문자열.</param>
        /// <param name="displayableWidth">한 조각이 차지할 수 있는 최대 가로 크기(픽셀).</param>
        /// <param name="displayableHeight">한 조각이 차지할 수 있는 최대 세로 크기(픽셀).</param>
        /// <returns>순서대로 표시할 문자열 조각 목록.</returns>
        /// <remarks>
        /// 각 조각은 <see cref="TMP_Text.GetPreferredValues(string, float, float)"/>로 측정했을 때
        /// <paramref name="displayableWidth"/>·<paramref name="displayableHeight"/>를 넘지 않는 최대 길이로 잘립니다.
        /// 가능한 경우 공백·줄바꿈 앞에서 끊어 단어 중간 분할을 줄입니다.
        /// </remarks>
        public static List<string> SplitTextByDisplayableLength(
            TMP_Text textMeshPro,
            string text,
            float displayableWidth,
            float displayableHeight)
        {
            if (textMeshPro == null) throw new ArgumentNullException(nameof(textMeshPro));
            if (string.IsNullOrEmpty(text)) return new List<string>();

            // 측정 불가능한 영역이면 분할 없이 원문 전체를 한 덩어리로 반환합니다.
            if (displayableWidth <= PreferredSizeComparisonEpsilon
                || displayableHeight <= PreferredSizeComparisonEpsilon)
                return new List<string> { text };

            var displayableChunks = new List<string>();
            var sliceStartIndex = 0;

            while (sliceStartIndex < text.Length)
            {
                var remainingCharacterCount = text.Length - sliceStartIndex;

                // 이진 탐색으로 "영역 안에 들어가는 최대 글자 수"를 구합니다.
                var fittingCharacterCount = FindMaxFittingCharacterCount(
                    textMeshPro,
                    text,
                    sliceStartIndex,
                    remainingCharacterCount,
                    displayableWidth,
                    displayableHeight);

                // 단어·줄 중간보다 공백/줄바꿈 앞에서 끊는 편이 읽기 좋습니다.
                fittingCharacterCount = AdjustSliceEndToWordBoundary(text, sliceStartIndex, fittingCharacterCount);

                displayableChunks.Add(text.Substring(sliceStartIndex, fittingCharacterCount));
                sliceStartIndex += fittingCharacterCount;
            }

            return displayableChunks;
        }

        /// <summary>
        /// <paramref name="text"/>의 <paramref name="sliceStartIndex"/>부터
        /// 최대 <paramref name="maxCandidateCharacterCount"/>글자 중,
        /// <paramref name="displayableWidth"/>×<paramref name="displayableHeight"/> 안에 들어가는 가장 긴 길이를 반환합니다.
        /// </summary>
        private static int FindMaxFittingCharacterCount(
            TMP_Text textMeshPro,
            string text,
            int sliceStartIndex,
            int maxCandidateCharacterCount,
            float displayableWidth,
            float displayableHeight)
        {
            if (maxCandidateCharacterCount <= 0) return 0;

            var searchRangeLow = 1;
            var searchRangeHigh = maxCandidateCharacterCount;
            var maxFittingCharacterCount = 0;

            while (searchRangeLow <= searchRangeHigh)
            {
                var candidateCharacterCount = (searchRangeLow + searchRangeHigh) >> 1;

                if (DoesTextSliceFitInDisplayArea(
                        textMeshPro,
                        text,
                        sliceStartIndex,
                        candidateCharacterCount,
                        displayableWidth,
                        displayableHeight))
                {
                    maxFittingCharacterCount = candidateCharacterCount;
                    searchRangeLow = candidateCharacterCount + 1;
                }
                else
                {
                    searchRangeHigh = candidateCharacterCount - 1;
                }
            }

            // 한 글자도 영역보다 크면(폰트·설정 문제) 무한 루프를 막기 위해 최소 1글자는 진행합니다.
            return maxFittingCharacterCount > 0 ? maxFittingCharacterCount : 1;
        }

        /// <summary>
        /// 후보 문자열을 TMP로 레이아웃 측정했을 때 지정 영역을 넘지 않는지 확인합니다.
        /// </summary>
        private static bool DoesTextSliceFitInDisplayArea(
            TMP_Text textMeshPro,
            string text,
            int sliceStartIndex,
            int characterCount,
            float displayableWidth,
            float displayableHeight)
        {
            var candidateText = text.Substring(sliceStartIndex, characterCount);
            var preferredSize = textMeshPro.GetPreferredValues(candidateText, displayableWidth, displayableHeight);

            return preferredSize.x <= displayableWidth + PreferredSizeComparisonEpsilon
                && preferredSize.y <= displayableHeight + PreferredSizeComparisonEpsilon;
        }

        /// <summary>
        /// 이진 탐색으로 구한 잘림 위치를, 가능하면 줄바꿈·공백 직전으로 당깁니다.
        /// </summary>
        /// <param name="text">원본 문자열.</param>
        /// <param name="sliceStartIndex">현재 조각의 시작 인덱스.</param>
        /// <param name="fittingCharacterCount">영역에 맞는 최대 글자 수(조정 전).</param>
        /// <returns>실제로 잘라낼 글자 수.</returns>
        private static int AdjustSliceEndToWordBoundary(string text, int sliceStartIndex, int fittingCharacterCount)
        {
            // 마지막 조각이거나 한 글자뿐이면 조정하지 않습니다.
            if (fittingCharacterCount <= 1 || sliceStartIndex + fittingCharacterCount >= text.Length)
                return fittingCharacterCount;

            var sliceEndExclusiveIndex = sliceStartIndex + fittingCharacterCount;

            // 우선 명시적 줄바꿈(\n) 앞에서 끊습니다.
            var lastNewlineIndex = text.LastIndexOf('\n', sliceEndExclusiveIndex - 1, fittingCharacterCount);
            if (lastNewlineIndex >= sliceStartIndex)
                return lastNewlineIndex - sliceStartIndex + 1;

            // 다음으로 공백 앞에서 끊습니다. (공백 문자는 다음 조각으로 넘깁니다)
            var lastSpaceIndex = text.LastIndexOf(' ', sliceEndExclusiveIndex - 1, fittingCharacterCount);
            if (lastSpaceIndex > sliceStartIndex)
                return lastSpaceIndex - sliceStartIndex;

            return fittingCharacterCount;
        }
    }
}
