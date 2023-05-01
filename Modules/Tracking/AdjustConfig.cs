#if PANCAKE_ADJUST
using com.adjust.sdk;
#endif
using UnityEngine;

namespace Pancake.Tracking
{
    //[HideMonoScript]
    [EditorIcon("scriptable_adjust")]
    public class AdjustConfig : ScriptableSettings<AdjustConfig>
    {
        //[InfoBox("On iOS Adjust will be initial after ATT popup completed!")] 
        [SerializeField]
        private string appToken;

        public static string AppToken => Instance.appToken;

#if PANCAKE_ADJUST
        [SerializeField] private AdjustEnvironment environment = AdjustEnvironment.Production;
        [SerializeField] private AdjustLogLevel logLevel = AdjustLogLevel.Error;

        public static AdjustEnvironment Environment => Instance.environment;
        public static AdjustLogLevel LogLevel => Instance.logLevel;
        public static bool IsProductEnvironment => Environment == AdjustEnvironment.Production;
        public static bool IsErrorLogLevel => LogLevel == AdjustLogLevel.Error;
#endif
    }
}