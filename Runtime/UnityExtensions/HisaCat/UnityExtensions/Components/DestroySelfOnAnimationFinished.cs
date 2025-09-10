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
            yield return HisaCat.CachedYieldInstruction.WaitForEndOfFrame();
            while (this.m_Animation.isPlaying) yield return HisaCat.CachedYieldInstruction.WaitForEndOfFrame();
            Destroy(this.gameObject);
            yield break;
        }
    }
}
