
using System.Runtime.CompilerServices;
using UnityEngine;

namespace HisaCat.HUE.UnityExtensions
{
    public static class ManagedConditionLog
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Log(bool condition, string message)
        {
            if (condition) { ManagedDebug.Log(message); return true; }
            return false;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Log(bool condition, string message, Object context)
        {
            if (condition) { ManagedDebug.Log(message, context); return true; }
            return false;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool LogWarning(bool condition, string message)
        {
            if (condition) { ManagedDebug.LogWarning(message); return true; }
            return false;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool LogWarning(bool condition, string message, Object context)
        {
            if (condition) { ManagedDebug.LogWarning(message, context); return true; }
            return false;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool LogError(bool condition, string message)
        {
            if (condition) { ManagedDebug.LogError(message); return true; }
            return false;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool LogError(bool condition, string message, Object context)
        {
            if (condition) { ManagedDebug.LogError(message, context); return true; }
            return false;
        }
    }
}
