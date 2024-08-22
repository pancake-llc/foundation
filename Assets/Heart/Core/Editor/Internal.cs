using System;
using PancakeEditor.Common;

namespace PancakeEditor
{
    internal static class Internal
    {
        private static UserSetting<UserInternalData> internalData;

        internal static UserSetting<UserInternalData> InternalData
        {
            get
            {
                if (internalData != null) return internalData;
                internalData = new UserSetting<UserInternalData>();
                internalData.LoadSetting();
                return internalData;
            }
        }
    }

    [Serializable]
    internal class UserInternalData
    {
        /// <summary>
        /// Show a window to ask whether to save the scene or not?
        /// </summary>
        public bool requireSceneSave;
    }
}