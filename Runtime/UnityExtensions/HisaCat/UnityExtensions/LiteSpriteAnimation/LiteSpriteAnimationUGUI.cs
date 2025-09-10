using UnityEngine;
using UnityEngine.UI;

namespace HisaCat
{
    public class LiteSpriteAnimationUGUI : LiteSpriteAnimationCore
    {
        public Image Image => this.m_Image;
        [SerializeField] Image m_Image = null;

        protected void Reset()
        {
            if (this.m_Image == null)
                this.m_Image = GetComponent<Image>();
            if (this.m_Image == null)
                this.m_Image = GetComponentInChildren<Image>();
        }

        protected override void ApplySprite(Sprite sprite)
        {
            this.m_Image.sprite = sprite;
        }
    }
}
