#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;

namespace HisaCat.EditorUtils
{
    public static class FixFloatFieldPrecision
    {
        private const string MenuPath_BoxCollider_BoundSize = "HisaCat/Fix Float Field Precision/Box Collider Bound Size";
        [MenuItem(MenuPath_BoxCollider_BoundSize)]
        public static void FixBoxColliderFloatFieldPrecision(MenuCommand command)
        {
            // Block function call multiple times when multiple object selected.
            if (command.context != null && Selection.gameObjects.Length > 1 && Selection.gameObjects[0] != command.context)
                return;

            var targets = Selection.gameObjects.Select(e => e.GetComponent<BoxCollider>()).Where(e => e != null).ToArray();
            foreach (var target in targets)
            {
                Undo.RecordObject(target, "Fix Box Collider Bound Size");
                var center = target.center;
                center.x = FixFloatPrecisionSmart(center.x);
                center.y = FixFloatPrecisionSmart(center.y);
                center.z = FixFloatPrecisionSmart(center.z);
                target.center = center;

                var size = target.size;
                size.x = FixFloatPrecisionSmart(size.x);
                size.y = FixFloatPrecisionSmart(size.y);
                size.z = FixFloatPrecisionSmart(size.z);
                target.size = size;
            }
        }
        [MenuItem(MenuPath_BoxCollider_BoundSize, validate = true)]
        public static bool FixBoxColliderFloatFieldPrecisionValidate()
        {
            if (Selection.gameObjects.All(e => e.GetComponent<BoxCollider>() == null))
                return false;
            return true;
        }

        /// <summary>
        /// “보기에 지저분한” 0/9 꼬리를 epsilon 범위 내에서만 정리합니다.
        /// - 0..maxDecimals 자리 중 가장 작은 자리수로 깔끔히 떨어지면 그 자리수로 고정
        /// - 허용오차 밖이면 변경하지 않음
        /// </summary>
        public static float FixFloatPrecisionSmart(
            float value,
            int maxDecimals = 6,
            float absTolerance = 1e-7f,     // 절대 오차
            float relTolerance = 1e-6f      // 상대 오차 (값의 크기에 비례)
        )
        {
            if (float.IsNaN(value) || float.IsInfinity(value))
                return value;

            // 음수 zero 정리 방지용(마지막에 한 번 더 0 스냅)
            float original = value;

            // 0자리(정수) ~ maxDecimals까지 늘려가며 가장 ‘낮은 자리’에서 만족하는 반올림을 채택
            for (int n = 0; n <= maxDecimals; n++)
            {
                // AwayFromZero: 0.5 경계에서 사람 기대와 일치
                double rounded = Math.Round(value, n, MidpointRounding.AwayFromZero);
                float candidate = (float)rounded;

                if (NearlyEqual(value, candidate, absTolerance, relTolerance))
                {
                    // -0.0f 방지용
                    if (Mathf.Abs(candidate) < absTolerance) candidate = 0f;
                    return candidate;
                }
            }

            // 어떤 자리도 허용 오차 안에 들지 않으면 변경하지 않음
            // Debug.LogWarning($"[FixFloatPrecisionSmart] 허용오차 초과: {value}");
            // -0.0f 정리
            if (Mathf.Abs(original) < absTolerance) return 0f;
            return original;
        }

        // 절대/상대 오차 조합 비교
        private static bool NearlyEqual(float a, float b, float absTol, float relTol)
        {
            float diff = Mathf.Abs(a - b);
            if (diff <= absTol) return true;

            float scale = Mathf.Max(1f, Mathf.Abs(a), Mathf.Abs(b));
            return diff <= relTol * scale;
        }
    }
}
#endif
