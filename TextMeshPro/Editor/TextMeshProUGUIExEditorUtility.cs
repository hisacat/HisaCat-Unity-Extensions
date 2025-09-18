#if HUE_UGUI_TMPRO_INCLUDED
using UnityEngine;
using UnityEditor;
using TMPro;

namespace HisaCat.HUE
{
    public static class TextMeshProUGUIExEditorUtility
    {
        private const string ConvertToTextMeshProUGUIExItemName = "CONTEXT/TextMeshProUGUI/Convert to TextMeshProUGUIEx";
        private const string RestoreToTextMeshProUGUIItemName = "CONTEXT/TextMeshProUGUI/Restore to TextMeshProUGUI";

        [MenuItem(ConvertToTextMeshProUGUIExItemName)]
        private static void ConvertToTextMeshProUGUIEx(MenuCommand command)
        {
            var tmpro = command.context as TextMeshProUGUI;
            if (tmpro == null) return;
            ComponentConverter.ConvertComponent<TextMeshProUGUI, TextMeshProUGUIEx>(tmpro);
        }

        [MenuItem(RestoreToTextMeshProUGUIItemName)]
        private static void RestoreToTextMeshProUGUI(MenuCommand command)
        {
            var tmproEx = command.context as TextMeshProUGUIEx;
            if (tmproEx == null) return;
            ComponentConverter.ConvertComponent<TextMeshProUGUIEx, TextMeshProUGUI>(tmproEx);
        }

        [MenuItem(ConvertToTextMeshProUGUIExItemName, true)]
        private static bool ValidateConvertToTextMeshProUGUIEx(MenuCommand command)
        {
            var tmpro = command.context as TextMeshProUGUI;
            if (tmpro == null) return false;

            var type = tmpro.GetType();
            return type == typeof(TextMeshProUGUI);
        }

        [MenuItem(RestoreToTextMeshProUGUIItemName, true)]
        private static bool ValidateRestoreToTextMeshProUGUI(MenuCommand command)
        {
            var tmproEx = command.context as TextMeshProUGUIEx;
            if (tmproEx == null) return false;

            var type = tmproEx.GetType();
            return type == typeof(TextMeshProUGUIEx);
        }
    }
}
#endif
