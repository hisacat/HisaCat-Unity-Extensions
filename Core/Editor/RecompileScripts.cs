using UnityEditor;
using UnityEditor.Compilation;

namespace HisaCat.HUE.Editors
{
    public static class RecompileScripts
    {
        [MenuItem("HisaCat/HUE/Recompile Scripts")]
        public static void DoRecompileScripts()
        {
            CompilationPipeline.RequestScriptCompilation(RequestScriptCompilationOptions.CleanBuildCache);
        }
    }
}
