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
    }
}