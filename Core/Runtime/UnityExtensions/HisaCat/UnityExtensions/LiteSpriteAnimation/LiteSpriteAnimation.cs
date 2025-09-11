using UnityEngine;

namespace HisaCat
{
    public class LiteSpriteAnimation : LiteSpriteAnimationCore
    {
        public SpriteRenderer SpriteRenderer => this.m_SpriteRenderer;
        [SerializeField] SpriteRenderer m_SpriteRenderer = null;

        protected void Reset()
        {
            if (this.m_SpriteRenderer == null)
                this.m_SpriteRenderer = GetComponent<SpriteRenderer>();
            if (this.m_SpriteRenderer == null)
                this.m_SpriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        protected override void ApplySprite(Sprite sprite)
        {
            this.m_SpriteRenderer.sprite = sprite;
        }
    }
}
