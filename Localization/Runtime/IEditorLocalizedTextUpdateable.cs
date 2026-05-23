namespace HisaCat.HUE.Localization
{
    public interface IEditorLocalizedTextUpdateable
    {
#if UNITY_EDITOR
        void UpdateTextOnEditor();
#endif
    }
}
