using System;
using System.Collections.Generic;

namespace Pancake.Editor
{
    [Serializable]
    internal class PathSetting
    {
        public List<string> whitelistPaths;
        public List<string> blacklistPaths;

        public PathSetting()
        {
            whitelistPaths = new List<string>();
            blacklistPaths = new List<string>();
        }
    }
}