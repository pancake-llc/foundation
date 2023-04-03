using Pancake;
using Pancake.Attribute;
using Pancake.Tracking;
using UnityEngine;

namespace PancakeEditor
{
    [EditorIcon("scriptable_build")]
    public class EditorPreBuildValidateAdjust : EditorPreBuildCondition
    {
        public override (bool, string) Validate()
        {
#if !PANCAKE_ADJUST
            return (true, "");
#else
            var adjustSetting = Resources.Load<AdjustConfig>(nameof(AdjustConfig));
            if (adjustSetting == null)
            {
                return (false, "Adjust was installed but AdjustConfig can not be found!");
            }

            if (string.IsNullOrEmpty(AdjustConfig.AppToken))
            {
                return (false, "AppToken can not be empty");
            }

            if (EditorPreBuildSettings.AppBundle)
            {
                // for release
                if (!AdjustConfig.IsProductEnvironment)
                {
                    return (false,
                        "For a build marked as app bundle (which is a release) you need to change the environment of adjust to Production");
                }

                if (!AdjustConfig.IsErrorLogLevel)
                {
                    return (false,
                        "For a build marked as app bundle (which is a release) you need to change the log level of adjust to Error");
                }
            }

            return (true, "");
#endif
        }
    }
}