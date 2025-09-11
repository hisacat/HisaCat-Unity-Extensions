using System.Runtime.CompilerServices;
using UnityEngine;

namespace HisaCat.UnityExtensions
{
    /// <summary>
    /// Easing functions.<br/>
    /// See: https://easings.net/ko
    /// </summary>
    public static class Easings
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float EaseInSine(float t) => 1 - Mathf.Cos(t * Mathf.PI / 2);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float EaseOutSine(float t) => Mathf.Sin(t * Mathf.PI / 2);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float EaseInOutSine(float t) => -(Mathf.Cos(Mathf.PI * t) - 1) / 2;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float EaseInQuad(float t) => t * t;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float EaseOutQuad(float t) => t * (2 - t);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float EaseInOutQuad(float t) => t < 0.5 ? 2 * t * t : -2 * t * t + 4 * t - 1;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float EaseInCubic(float t) => t * t * t;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float EaseOutCubic(float t) => 1 - Mathf.Pow(1 - t, 3);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float EaseInOutCubic(float t) => t < 0.5 ? 4 * t * t * t : 1 - Mathf.Pow(-2 * t + 2, 3) / 2;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float EaseInQuart(float t) => t * t * t * t;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float EaseOutQuart(float t) => 1 - Mathf.Pow(1 - t, 4);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float EaseInOutQuart(float t) => t < 0.5 ? 8 * t * t * t * t : 1 - Mathf.Pow(-2 * t + 2, 4) / 2;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float EaseInQuint(float t) => t * t * t * t * t;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float EaseOutQuint(float t) => 1 - Mathf.Pow(1 - t, 5);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float EaseInOutQuint(float t) => t < 0.5 ? 16 * t * t * t * t * t : 1 - Mathf.Pow(-2 * t + 2, 5) / 2;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float EaseInExpo(float t) => 1 - Mathf.Pow(2, -10 * t);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float EaseOutExpo(float t) => Mathf.Pow(2, 10 * (t - 1));
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float EaseInOutExpo(float t) => t < 0.5 ? 1 / 2 * Mathf.Pow(2, 10 * (2 * t - 1)) : -1 / 2 * Mathf.Pow(2, -10 * (2 * t - 1)) + 1;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float EaseInCirc(float t) => 1 - Mathf.Sqrt(1 - t * t);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float EaseOutCirc(float t) => Mathf.Sqrt(1 - (t - 1) * (t - 1));
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float EaseInOutCirc(float t) => t < 0.5 ? (1 - Mathf.Sqrt(1 - Mathf.Pow(2 * t, 2))) / 2 : (Mathf.Sqrt(1 - Mathf.Pow(-2 * t + 2, 2)) + 1) / 2;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float EaseInBack(float t) => t * t * (2.70158f * t - 1.70158f);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float EaseOutBack(float t) => 1 + 2.70158f * Mathf.Pow(t - 1, 3);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float EaseInOutBack(float t) => t < 0.5 ? (Mathf.Pow(2 * t, 2) * ((1.70158f * (t * 2 - 1)) + 1)) / 2 : ((Mathf.Pow(2 * t - 2, 2) * ((1.70158f * (t * 2 - 3)) + 2)) + 2) / 2;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float EaseInElastic(float t) => Mathf.Pow(2, -10 * t) * Mathf.Sin((t - 0.075f) * (2 * Mathf.PI) / 0.3f) + 1;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float EaseOutElastic(float t) => Mathf.Pow(2, -10 * t) * Mathf.Sin((t - 0.075f) * (2 * Mathf.PI) / 0.3f) * 0.5f + 1;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float EaseInOutElastic(float t) => t < 0.5 ? 1 / 2 * Mathf.Pow(2, -10 * t) * Mathf.Sin((t - 0.075f) * (2 * Mathf.PI) / 0.3f) + 1 : -1 / 2 * Mathf.Pow(2, -10 * t) * Mathf.Sin((t - 0.075f) * (2 * Mathf.PI) / 0.3f) * 0.5f + 1;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float EaseInBounce(float t) => 1 - EaseOutBounce(1 - t);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float EaseOutBounce(float t) => 1 - Mathf.Pow(1 - t, 2);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float EaseInOutBounce(float t) => t < 0.5 ? 1 / 2 * (1 - EaseOutBounce(1 - t * 2)) : 1 / 2 * EaseOutBounce(t * 2 - 1) + 1 / 2;
    }
}
