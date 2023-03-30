#if PANCAKE_ADJUST
using com.adjust.sdk;
#endif
using Pancake.Attribute;
using UnityEngine;

namespace Pancake.Tracking
{
    [HideMono]
    [EditorIcon("scriptable_adjust")]
    public class AdjustConfig : ScriptableSettings<AdjustConfig>
    {
        [InfoBox("On iOS Adjust will be initial after ATT popup completed!")] [SerializeField]
        private string appToken;

        public static string AppToken => Instance.appToken;
        
#if PANCAKE_ADJUST
        [SerializeField] private AdjustEnvironment environment = AdjustEnvironment.Production;
        [SerializeField] private AdjustLogLevel logLevel = AdjustLogLevel.Error;
        
        public static AdjustEnvironment Environment => Instance.environment;
        public static AdjustLogLevel LogLevel => Instance.logLevel;
#endif
    }
}