using System.IO;
using UnityEditor;
using UnityEngine;

namespace HisaCat.Editors
{
    public static class CameraCapture
    {
        [MenuItem("HisaCat/Utils/Camera Capture")]
        public static void CaptureAndSave()
        {
            var camObj = Selection.activeGameObject;
            var camera = camObj?.GetComponent<Camera>();
            if (camera == null)
            {
                EditorUtility.DisplayDialog("Camera Capture", "Please select camera gameobject", "Ok");
                return;
            }

            var path = EditorUtility.SaveFilePanel("Camera Capture", "", "capture", "png");
            if (string.IsNullOrEmpty(path))
            {
                EditorUtility.DisplayDialog("Camera Capture", "Canceled", "Ok");
                return;
            }

            byte[] bytes = Capture(camera, Screen.width, Screen.height);
            File.WriteAllBytes(path, bytes);

            EditorUtility.DisplayDialog("Camera Capture", "Done", "Ok");
        }

        public static byte[] Capture(Camera camera, int width, int height, System.Action prepareRender = null)
        {
            RenderTexture originActiveRenderTexture = RenderTexture.active;
            RenderTexture camOriginRenderTexture = camera.targetTexture;

            if (camera.targetTexture == null)
                camera.targetTexture = new RenderTexture(width, height, 32);

            RenderTexture.active = camera.targetTexture;

            prepareRender?.Invoke();
            camera.Render();

            Texture2D image = new Texture2D(camera.targetTexture.width, camera.targetTexture.height);
            image.ReadPixels(new Rect(0, 0, camera.targetTexture.width, camera.targetTexture.height), 0, 0);
            image.Apply();

            RenderTexture.active = originActiveRenderTexture;
            camera.targetTexture = camOriginRenderTexture;

            byte[] bytes = image.EncodeToPNG();
            if (Application.isPlaying)
                GameObject.Destroy(image);
            else
                GameObject.DestroyImmediate(image);

            return bytes;
        }
    }
}
