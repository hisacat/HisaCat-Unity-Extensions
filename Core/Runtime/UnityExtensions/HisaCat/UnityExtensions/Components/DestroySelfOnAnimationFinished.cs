using System.Collections;
using UnityEngine;

namespace HisaCat
{
    public class DestroySelfOnAnimationFinished : MonoBehaviour
    {
        [SerializeField] private Animation m_Animation;
        private void Reset()
        {
            this.m_Animation = GetComponent<Animation>();
        }
        private void Awake()
        {
            StartCoroutine(DestroyRoutine());
        }

        private IEnumerator DestroyRoutine()
        {
            // Wait until animation starts
            yield return new WaitUntil(IsAnimationPlaying);
            bool IsAnimationPlaying() => this.m_Animation.isPlaying;

            // Wait until animation finishes
            yield return new WaitUntil(IsAnimationFinished);
            bool IsAnimationFinished() => this.m_Animation.isPlaying == false;

            Destroy(this.gameObject);
            yield break;
        }
    }
}
