using System.Collections;
using UnityEngine;

namespace HisaCat
{
    public static class AnimationPlayUnscaledTime
    {
        public static IEnumerator PlayUnscaledTimeRoutine(this Animation animation, string clipName)
        {
            return PlayAnimationUnscaledTimeRoutine(animation, clipName);
        }
        public static IEnumerator PlayAnimationUnscaledTimeRoutine(Animation animation, string clipName)
        {
            AnimationState _currState = animation[clipName];
#pragma warning disable CS0219
            bool isPlaying = true;
            float _startTime = 0F;
            float _progressTime = 0F;
            float _timeAtLastFrame = 0F;
            float _timeAtCurrentFrame = 0F;
            float deltaTime = 0F;
#pragma warning restore CS0219

            animation.Play(clipName);
            _currState.speed = 0; //NOTE: Dont make it updated self by effected on timescale.

            _timeAtLastFrame = Time.realtimeSinceStartup;
            while (isPlaying)
            {
                if (_currState == null) break; //NOTE: fix for object destrying during animation.

                _timeAtCurrentFrame = Time.realtimeSinceStartup;
                deltaTime = _timeAtCurrentFrame - _timeAtLastFrame;
                _timeAtLastFrame = _timeAtCurrentFrame;

                _progressTime += deltaTime;
                _currState.normalizedTime = _progressTime / _currState.length;
                animation.Sample();

                if (_progressTime >= _currState.length)
                {
                    if (_currState.wrapMode != WrapMode.Loop)
                    {
                        isPlaying = false;
                    }
                    else
                    {
                        _progressTime = 0.0f;
                    }
                }

                yield return HisaCat.CachedYieldInstruction.WaitForEndOfFrame();
            }
            yield return null;
        }
    }
}
