using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pancake.SaveData
{
    internal static class Console
    {
        private const string DISABLE_INFO_MSG = "\n<i>To disable these messages from Archive, go to Tools > Pancake > Archive > Settings, and uncheck 'Log Info'</i>";
        private const string DISABLE_WARNING_MSG = "\n<i>To disable warnings from Archive, go to Tools > Pancake > Archive > Settings, and uncheck 'Log Warnings'</i>";
        private const string DISABLE_ERROR_MSG = "\n<i>To disable these error messages from Archive, go to Tools > Pancake > Archive > Settings, and uncheck 'Log Errors'</i>";

        private const char INDENT_CHAR = '-';

        public static void Log(string msg, Object context = null, int indent=0)
        {
            if (!MetaData.DefaultSetting.logInfo) return;
            if (context != null) Debug.LogFormat(context, Indent(indent) + msg + DISABLE_INFO_MSG);
            else Debug.LogFormat(context, Indent(indent) + msg);
        }

        public static void LogWarning(string msg, Object context=null, int indent = 0)
        {
            if (!MetaData.DefaultSetting.logWarnings) return;
            if (context != null) Debug.LogWarningFormat(context, Indent(indent) + msg + DISABLE_WARNING_MSG);
            else Debug.LogWarningFormat(context, Indent(indent) + msg + DISABLE_WARNING_MSG);
        }

        public static void LogError(string msg, Object context = null, int indent = 0)
        {
            if (!MetaData.DefaultSetting.logErrors) return;
            if (context != null) Debug.LogErrorFormat(context, Indent(indent) + msg + DISABLE_ERROR_MSG);
            else Debug.LogErrorFormat(context, Indent(indent) + msg + DISABLE_ERROR_MSG);
        }

        private static string Indent(int size)
        {
            if (size < 0) return "";
            return new string(INDENT_CHAR, size);
        }
    }
}
