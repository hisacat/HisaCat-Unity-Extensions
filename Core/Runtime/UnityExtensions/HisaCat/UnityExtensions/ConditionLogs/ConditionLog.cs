using System.Runtime.CompilerServices;
using UnityEngine;

namespace HisaCat.UnityExtensions
{
    public static class ConditionLog
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Log(bool condition, string message)
        {
            if (condition) { Debug.Log(message); return true; }
            return false;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Log(bool condition, string message, Object context)
        {
            if (condition) { Debug.Log(message, context); return true; }
            return false;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool LogWarning(bool condition, string message)
        {
            if (condition) { Debug.LogWarning(message); return true; }
            return false;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool LogWarning(bool condition, string message, Object context)
        {
            if (condition) { Debug.LogWarning(message, context); return true; }
            return false;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool LogError(bool condition, string message)
        {
            if (condition) { Debug.LogError(message); return true; }
            return false;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool LogError(bool condition, string message, Object context)
        {
            if (condition) { Debug.LogError(message, context); return true; }
            return false;
        }
    }
}
