#if true && (UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_5 || UNITY_2017_1_OR_NEWER) // MODULE_MARKER
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using HisaCat.SpriteEx;
using UnityEngine;

#pragma warning disable 1591
namespace DG.Tweening
{
    public static class DOTweenModuleSpriteEx
    {
        #region Shortcuts

        #region SpriteRenderer

        public static TweenerCore<Color, Color, ColorOptions> DOFillColor(this SpriteExSpriteRenderer target, Color endValue, float duration)
        {
            TweenerCore<Color, Color, ColorOptions> t = DOTween.To(() => target.fillColor, x => target.fillColor = x, endValue, duration);
            t.SetTarget(target);
            return t;
        }

        public static TweenerCore<Color, Color, ColorOptions> DOFillFade(this SpriteExSpriteRenderer target, float endValue, float duration)
        {
            TweenerCore<Color, Color, ColorOptions> t = DOTween.ToAlpha(() => target.fillColor, x => target.fillColor = x, endValue, duration);
            t.SetTarget(target);
            return t;
        }

        public static Sequence DOGradientFillColor(this SpriteExSpriteRenderer target, Gradient gradient, float duration)
        {
            Sequence s = DOTween.Sequence();
            GradientColorKey[] colors = gradient.colorKeys;
            int len = colors.Length;
            for (int i = 0; i < len; ++i)
            {
                GradientColorKey c = colors[i];
                if (i == 0 && c.time <= 0)
                {
                    target.fillColor = c.color;
                    continue;
                }
                float colorDuration = i == len - 1
                    ? duration - s.Duration(false) // Verifies that total duration is correct
                    : duration * (i == 0 ? c.time : c.time - colors[i - 1].time);
                s.Append(target.DOFillColor(c.color, colorDuration).SetEase(Ease.Linear));
            }
            s.SetTarget(target);
            return s;
        }

        #endregion

        #region Blendables

        #region SpriteRenderer

        public static Tweener DOBlendableFillColor(this SpriteExSpriteRenderer target, Color endValue, float duration)
        {
            endValue = endValue - target.fillColor;
            Color to = new Color(0, 0, 0, 0);
            return DOTween.To(() => to, x =>
            {
                Color diff = x - to;
                to = x;
                target.fillColor += diff;
            }, endValue, duration)
                .Blendable().SetTarget(target);
        }

        #endregion

        #endregion

        #endregion
    }
}
#endif
