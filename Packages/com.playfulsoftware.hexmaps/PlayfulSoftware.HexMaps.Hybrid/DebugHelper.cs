using UnityEngine;

namespace PlayfulSoftware.HexMaps.Hybrid
{
    static class DebugHelper
    {
        public static void LogNoStacktrace(string msg)
        {
            var traceLevel = Application.GetStackTraceLogType(LogType.Log);
            Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
            Debug.Log(msg);
            Application.SetStackTraceLogType(LogType.Log, traceLevel);
        }

        public static void LogErrorNoStacktrace(string msg)
        {
            var traceLevel = Application.GetStackTraceLogType(LogType.Error);
            Application.SetStackTraceLogType(LogType.Error, StackTraceLogType.None);
            Debug.Log(msg);
            Application.SetStackTraceLogType(LogType.Error, traceLevel);
        }
    }
}