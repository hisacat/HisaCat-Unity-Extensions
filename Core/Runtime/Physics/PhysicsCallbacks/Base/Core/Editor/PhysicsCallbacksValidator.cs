#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

namespace HisaCat.HUE.PhysicsExtension
{
    [InitializeOnLoad]
    internal static class PhysicsCallbacksValidator
    {
        private static bool wasValidated = false;
        static PhysicsCallbacksValidator()
        {
            // afterAssemblyReload is called earlier than compilationFinished.
            AssemblyReloadEvents.afterAssemblyReload -= OnAfterAssemblyReload;
            AssemblyReloadEvents.afterAssemblyReload += OnAfterAssemblyReload;

            CompilationPipeline.compilationFinished -= OnCompilationFinished;
            CompilationPipeline.compilationFinished += OnCompilationFinished;

            static void OnAfterAssemblyReload()
            {
                if (wasValidated) return;

                ValidatePhysicsCallbacksScripts();
                wasValidated = true;
            }
            static void OnCompilationFinished(object obj) => wasValidated = false;
        }

        private static readonly Type TargetBaseType = typeof(ReliablePhysicsCallbacksCore<,>);
        public static void ValidatePhysicsCallbacksScripts()
        {
            var allMonoScripts = MonoImporter.GetAllRuntimeMonoScripts();

            foreach (var monoScript in allMonoScripts)
            {
                var type = monoScript.GetClass();
                if (type == null) continue;

                // Ignore abstract / generic definition
                if (type.IsAbstract || type.ContainsGenericParameters)
                    continue;

                // Check if the type inherits from the target base type
                if (IsSubclassOfRawGeneric(TargetBaseType, type) == false)
                    continue;

                // Check if the type has the DefaultExecutionOrder attribute
                if (Attribute.IsDefined(type, typeof(DefaultExecutionOrder)) == false)
                {
                    Debug.LogWarning(
                        $"\"{type.Name}\" is derived from \"{TargetBaseType.Name}\" but {nameof(DefaultExecutionOrder)} attribute is not set." +
                        $"\r\nIt recommended to set {nameof(DefaultExecutionOrder)} attribute with a sufficiently small value." +
                        $"\r\n e.g.: [DefaultExecutionOrder(int.MinValue)]",
                        monoScript);
                }
            }
        }

        /// <summary>
        /// Check if the type is a subclass of a raw generic type
        /// </summary>
        /// <param name="generic">The generic type to check against</param>
        /// <param name="toCheck">The type to check</param>
        /// <returns>True if the type is a subclass of the generic type, false otherwise</returns>
        private static bool IsSubclassOfRawGeneric(Type generic, Type toCheck)
        {
            while (toCheck != null && toCheck != typeof(object))
            {
                var cur = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
                if (cur == generic)
                    return true;
                toCheck = toCheck.BaseType;
            }
            return false;
        }
    }
}
#endif
