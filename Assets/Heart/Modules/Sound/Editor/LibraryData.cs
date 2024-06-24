using System;
using System.Collections.Generic;
using PancakeEditor.Common;

namespace PancakeEditor.Sound
{
    [Serializable]
    internal class LibraryData
    {
        public string assetOutputPath = string.Empty;
        public List<string> guids = new();
    }

    internal static class LibraryDataContainer
    {
        private static ProjectSetting<LibraryData> data;

        internal static ProjectSetting<LibraryData> Data
        {
            get
            {
                if (data != null) return data;
                data = new ProjectSetting<LibraryData>();
                data.LoadSetting();
                return data;
            }
        }
    }
}