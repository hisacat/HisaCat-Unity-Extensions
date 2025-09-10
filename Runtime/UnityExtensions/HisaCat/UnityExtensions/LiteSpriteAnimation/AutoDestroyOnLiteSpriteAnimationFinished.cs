using UnityEngine;

namespace HisaCat
{
    public class AutoDestroyOnLiteSpriteAnimationFinished : MonoBehaviour
    {
        [SerializeField] private LiteSpriteAnimation m_Animation = null;
        private void Reset()
        {
            if (this.m_Animation == null)
                this.m_Animation = GetComponent<LiteSpriteAnimation>();
        }

        private void Update()
        {
            if (this.m_Animation.NormalizedTime >= 1f)
                Destroy(this.gameObject);
        }
    }
}
