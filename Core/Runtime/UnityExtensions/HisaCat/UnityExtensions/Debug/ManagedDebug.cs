using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

public static class ManagedDebug
{
    public const string LogConditionString = "DEVELOPMENT_BUILD";
    public const string EditorConditionString = "UNITY_EDITOR";

    [Conditional(LogConditionString), Conditional(EditorConditionString)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Log(object message, Object context)
    {
        Debug.Log(message, context);
    }

    [Conditional(LogConditionString), Conditional(EditorConditionString)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Log(object message)
    {
        Debug.Log(message);
    }

    [Conditional(LogConditionString), Conditional(EditorConditionString)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void LogError(object message, Object context)
    {
        Debug.LogError(message, context);
    }

    [Conditional(LogConditionString), Conditional(EditorConditionString)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void LogError(object message)
    {
        Debug.LogError(message);
    }

    [Conditional(LogConditionString), Conditional(EditorConditionString)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void LogErrorFormat(string format, params object[] args)
    {
        Debug.LogErrorFormat(format, args);
    }

    [Conditional(LogConditionString), Conditional(EditorConditionString)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void LogErrorFormat(Object context, string format, params object[] args)
    {
        Debug.LogErrorFormat(context, format, args);
    }

    [Conditional(LogConditionString), Conditional(EditorConditionString)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void LogException(Exception exception)
    {
        Debug.LogException(exception);
    }

    [Conditional(LogConditionString), Conditional(EditorConditionString)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void LogException(Exception exception, Object context)
    {
        Debug.LogException(exception, context);
    }

    [Conditional(LogConditionString), Conditional(EditorConditionString)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void LogFormat(LogType logType, LogOption logOptions, Object context, string format, params object[] args)
    {
        Debug.LogFormat(logType, logOptions, context, format, args);
    }

    [Conditional(LogConditionString), Conditional(EditorConditionString)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void LogFormat(Object context, string format, params object[] args)
    {
        Debug.LogFormat(context, format, args);
    }

    [Conditional(LogConditionString), Conditional(EditorConditionString)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void LogFormat(string format, params object[] args)
    {
        Debug.LogFormat(format, args);
    }

    [Conditional(LogConditionString), Conditional(EditorConditionString)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void LogWarning(object message)
    {
        Debug.LogWarning(message);
    }

    [Conditional(LogConditionString), Conditional(EditorConditionString)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void LogWarning(object message, Object context)
    {
        Debug.LogWarning(message, context);
    }

    [Conditional(LogConditionString), Conditional(EditorConditionString)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void LogWarningFormat(string format, params object[] args)
    {
        Debug.LogWarningFormat(format, args);
    }

    [Conditional(LogConditionString), Conditional(EditorConditionString)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void LogWarningFormat(Object context, string format, params object[] args)
    {
        Debug.LogWarningFormat(context, format, args);
    }
}
