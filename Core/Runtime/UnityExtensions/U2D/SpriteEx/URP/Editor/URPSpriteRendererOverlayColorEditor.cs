#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace HisaCat.Shaders.URP.U2D
{
    [CustomEditor(typeof(SpriteRendererOverlayColor))]
    public class SpriteRendererOverlayColorEditor : Editor
    {
        // Or editor update. apply color on editor even only game scene without scene window.
        private void OnSceneGUI()
        {
            // Apply colors only on editor without play mode.
            if (Application.isPlaying) return;

//fade window for scene transition 만들어야함.
        }
    }
}
#endif
