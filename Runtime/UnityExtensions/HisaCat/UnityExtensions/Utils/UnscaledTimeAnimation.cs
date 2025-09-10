using System.Collections;
using UnityEngine;

namespace HisaCat
{
    [RequireComponent(typeof(Animation))]
    public class UnscaledTimeAnimation : MonoBehaviour
    {
        [SerializeField] private Animation m_Animation = null;
        public void Reset()
        {
            if (this.m_Animation == null)
                this.m_Animation = GetComponent<Animation>();
        }

        private Coroutine curPlayCoroutine = null;
        private void Start()
        {
            if (this.m_Animation.playAutomatically && this.m_Animation.clip != null)
            {
                if (this.curPlayCoroutine != null)
                    StopCoroutine(this.curPlayCoroutine);
                this.curPlayCoroutine = StartCoroutine(PlayRoutine());
            }
            else
            {
                Debug.LogWarning($"[{nameof(UnscaledTimeAnimation)}] Start: It only works for playAutomatically true and clip setted!");
            }
        }
        private IEnumerator PlayRoutine()
        {
            yield return AnimationPlayUnscaledTime.PlayAnimationUnscaledTimeRoutine(this.m_Animation, this.m_Animation.clip.name);
            this.curPlayCoroutine = null;
        }
    }
}
