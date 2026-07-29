using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NotNullAttribute = JetBrains.Annotations.NotNullAttribute;

namespace HisaCat
{
    /// <summary>
    /// Legacy <see cref="Animation"/> 컴포넌트용 코루틴 헬퍼입니다.
    /// </summary>
    /// <remarks>
    /// 클립 재생 완료까지 대기하거나, Time.timeScale의 영향을 받지 않는 unscaled 재생을 지원합니다.
    /// </remarks>
    public static class AnimationExtensions
    {
        /// <summary> <see cref="Animation.CrossFade(string, float)"/> 호출 시 fadeLength 인자를 생략했을 때와 동일한 기본값입니다. </summary>
        private const float DefaultCrossFadeLength = 0.3f;

        /// <summary>
        /// 지정한 클립을 재생하고, 재생이 끝날 때까지(Loop가 아닌 경우) 매 프레임 대기합니다.
        /// </summary>
        /// <param name="animation">대상 Animation 컴포넌트입니다.</param>
        /// <param name="clipName">재생할 AnimationState 이름입니다.</param>
        /// <returns>재생이 완료될 때까지 yield하는 코루틴입니다.</returns>
        /// <remarks>
        /// Time.timeScale의 영향을 받는 일반 재생입니다.
        /// 이미 <see cref="Animation.Play(string)"/>를 호출한 뒤 이 코루틴만 대기 용도로 사용해도 됩니다.
        /// </remarks>
        public static IEnumerator PlayRoutine([NotNull] this Animation animation, string clipName)
        {
            if (TryGetState(animation, clipName, out _) == false)
                yield break;

            animation.Play(clipName);

            // isPlaying은 CrossFade/Play 중 하나라도 활성이면 true입니다.
            while (animation != null && animation.isPlaying)
                yield return null;

            yield break;
        }

        /// <summary>
        /// Time.timeScale과 무관하게(unscaled) 지정한 클립을 재생하고, 재생이 끝날 때까지 대기합니다.
        /// </summary>
        /// <param name="animation">대상 Animation 컴포넌트입니다.</param>
        /// <param name="clipName">재생할 AnimationState 이름입니다.</param>
        /// <returns>재생이 완료될 때까지 yield하는 코루틴입니다.</returns>
        /// <remarks>
        /// 내부적으로 state.speed를 0으로 고정한 뒤 <see cref="Time.unscaledTime"/> 기준으로
        /// normalizedTime을 직접 갱신하고 <see cref="Animation.Sample"/>을 호출합니다.
        /// </remarks>
        public static IEnumerator PlayUnscaledTimeRoutine([NotNull] this Animation animation, string clipName)
        {
            if (TryGetState(animation, clipName, out AnimationState state) == false)
                yield break;

            // 클립 길이가 0이면 normalizedTime 계산에서 0으로 나누게 되므로 즉시 종료합니다.
            if (state.length <= 0f)
            {
                animation.Play(clipName);
                animation.Sample();
                yield break;
            }

            float originalSpeed = state.speed;

            try
            {
                animation.Play(clipName);

                // timeScale에 의해 자동 진행되지 않도록 speed를 0으로 고정합니다.
                state.speed = 0f;

                bool isPlaying = true;
                float progressTime = 0f;
                float timeAtLastFrame = Time.unscaledTime;

                while (isPlaying)
                {
                    // 재생 중 오브젝트가 파괴되면 state 참조가 무효화될 수 있습니다.
                    if (animation == null || state == null)
                        break;

                    float timeAtCurrentFrame = Time.unscaledTime;
                    float deltaTime = timeAtCurrentFrame - timeAtLastFrame;
                    timeAtLastFrame = timeAtCurrentFrame;

                    progressTime += deltaTime;
                    state.normalizedTime = progressTime / state.length;
                    animation.Sample();

                    if (progressTime >= state.length)
                    {
                        if (state.wrapMode == WrapMode.Loop)
                        {
                            // Loop 클립은 코루틴을 끝내지 않고 처음부터 다시 진행합니다.
                            progressTime = 0f;
                        }
                        else
                        {
                            // 마지막 프레임을 보장합니다.
                            state.normalizedTime = 1f;
                            animation.Sample();
                            isPlaying = false;
                        }
                    }

                    yield return CachedYieldInstruction.WaitForEndOfFrame();
                }
            }
            finally
            {
                // 이후 일반 Play/CrossFade가 정상 동작하도록 speed를 복원합니다.
                if (state != null)
                    state.speed = originalSpeed > 0f ? originalSpeed : 1f;
            }

            yield break;
        }

        /// <summary> 지정한 클립으로 CrossFade한 뒤, 재생이 끝날 때까지 대기합니다. </summary>
        /// <param name="animation">대상 Animation 컴포넌트입니다.</param>
        /// <param name="clipName">페이드 인할 AnimationState 이름입니다.</param>
        /// <param name="fadeLength">CrossFade에 사용할 페이드 시간(초)입니다.</param>
        /// <returns>CrossFade 및 재생이 완료될 때까지 yield하는 코루틴입니다.</returns>
        public static IEnumerator CrossFadeRoutine([NotNull] this Animation animation, string clipName, float fadeLength = DefaultCrossFadeLength)
        {
            if (TryGetState(animation, clipName, out _) == false)
                yield break;

            animation.CrossFade(clipName, fadeLength);

            // isPlaying은 CrossFade/Play 중 하나라도 활성이면 true입니다.
            while (animation != null && animation.isPlaying)
                yield return null;

            yield break;
        }

        /// <summary> Time.timeScale과 무관하게(unscaled) CrossFade한 뒤, 재생이 끝날 때까지 대기합니다. </summary>
        /// <param name="animation">대상 Animation 컴포넌트입니다.</param>
        /// <param name="clipName">페이드 인할 AnimationState 이름입니다.</param>
        /// <param name="fadeLength">CrossFade에 사용할 페이드 시간(초)입니다.</param>
        /// <returns>CrossFade 및 재생이 완료될 때까지 yield하는 코루틴입니다.</returns>
        public static IEnumerator CrossFadeUnscaledTimeRoutine([NotNull] this Animation animation, string clipName, float fadeLength = DefaultCrossFadeLength)
        {
            if (TryGetState(animation, clipName, out AnimationState targetState) == false)
                yield break;

            List<AnimationState> fadeOutStates = CollectFadeOutStates(animation, targetState);

            // 이전에 재생 중인 클립이 없으면 CrossFade 없이 unscaled Play만 수행합니다.
            if (fadeOutStates.Count == 0)
            {
                yield return PlayUnscaledTimeRoutine(animation, clipName);
                yield break;
            }

            float originalTargetSpeed = targetState.speed;
            float[] originalFadeOutSpeeds = new float[fadeOutStates.Count];
            for (int i = 0; i < fadeOutStates.Count; i++)
                originalFadeOutSpeeds[i] = fadeOutStates[i].speed;

            try
            {
                targetState.enabled = true;
                targetState.weight = 0f;
                targetState.normalizedTime = 0f;
                targetState.speed = 0f;

                for (int i = 0; i < fadeOutStates.Count; i++)
                    fadeOutStates[i].speed = 0f;

                float fadeDuration = Mathf.Max(0f, fadeLength);
                float fadeProgressTime = 0f;
                float timeAtLastFrame = Time.unscaledTime;

                // CrossFade 구간: weight를 realtime 기준으로 보간합니다.
                if (fadeDuration > 0f)
                {
                    while (fadeProgressTime < fadeDuration)
                    {
                        if (animation == null || targetState == null)
                            yield break;

                        float timeAtCurrentFrame = Time.unscaledTime;
                        float deltaTime = timeAtCurrentFrame - timeAtLastFrame;
                        timeAtLastFrame = timeAtCurrentFrame;

                        fadeProgressTime += deltaTime;
                        float t = Mathf.Clamp01(fadeProgressTime / fadeDuration);

                        targetState.weight = t;
                        for (int i = 0; i < fadeOutStates.Count; i++)
                        {
                            AnimationState fadeOutState = fadeOutStates[i];
                            if (fadeOutState == null)
                                continue;

                            fadeOutState.weight = 1f - t;

                            if (fadeOutState.length > 0f)
                                fadeOutState.normalizedTime += deltaTime / fadeOutState.length;
                        }

                        if (targetState.length > 0f)
                            targetState.normalizedTime += deltaTime / targetState.length;

                        animation.Sample();
                        yield return CachedYieldInstruction.WaitForEndOfFrame();
                    }
                }

                // 페이드 아웃 상태를 정리하고 타깃 클립만 남깁니다.
                for (int i = 0; i < fadeOutStates.Count; i++)
                {
                    AnimationState fadeOutState = fadeOutStates[i];
                    if (fadeOutState == null)
                        continue;

                    fadeOutState.enabled = false;
                    fadeOutState.weight = 0f;
                }

                targetState.weight = 1f;

                // 본편 구간: PlayUnscaledTimeRoutine과 동일한 방식으로 진행합니다.
                if (targetState.length <= 0f)
                {
                    animation.Sample();
                    yield break;
                }

                bool isPlaying = true;
                float progressTime = targetState.normalizedTime * targetState.length;
                timeAtLastFrame = Time.unscaledTime;

                while (isPlaying)
                {
                    if (animation == null || targetState == null)
                        break;

                    float timeAtCurrentFrame = Time.unscaledTime;
                    float deltaTime = timeAtCurrentFrame - timeAtLastFrame;
                    timeAtLastFrame = timeAtCurrentFrame;

                    progressTime += deltaTime;
                    targetState.normalizedTime = progressTime / targetState.length;
                    animation.Sample();

                    if (progressTime >= targetState.length)
                    {
                        if (targetState.wrapMode == WrapMode.Loop)
                        {
                            progressTime = 0f;
                        }
                        else
                        {
                            targetState.normalizedTime = 1f;
                            animation.Sample();
                            isPlaying = false;
                        }
                    }

                    yield return CachedYieldInstruction.WaitForEndOfFrame();
                }
            }
            finally
            {
                if (targetState != null)
                    targetState.speed = originalTargetSpeed > 0f ? originalTargetSpeed : 1f;

                for (int i = 0; i < fadeOutStates.Count; i++)
                {
                    AnimationState fadeOutState = fadeOutStates[i];
                    if (fadeOutState == null)
                        continue;

                    fadeOutState.speed = originalFadeOutSpeeds[i] > 0f ? originalFadeOutSpeeds[i] : 1f;
                }
            }

            yield break;
        }

        /// <summary> clipName에 해당하는 <see cref="AnimationState"/>를 가져옵니다. </summary>
        private static bool TryGetState([NotNull] Animation animation, string clipName, out AnimationState state)
        {
            state = null;

            if (string.IsNullOrEmpty(clipName)) return false;

            state = animation[clipName];
            return state != null;
        }

        /// <summary> CrossFade 시 페이드 아웃 대상이 되는, 현재 weight가 있는 상태 목록을 수집합니다. </summary>
        private static List<AnimationState> CollectFadeOutStates([NotNull] Animation animation, AnimationState targetState)
        {
            var fadeOutStates = new List<AnimationState>();

            foreach (AnimationState state in animation)
            {
                if (state == null || state == targetState)
                    continue;

                if (state.enabled == false || state.weight <= 0.001f)
                    continue;

                fadeOutStates.Add(state);
            }

            return fadeOutStates;
        }
    }
}
