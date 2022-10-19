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

    [Serializable]
    internal class AssetContainerSetting : PathSetting
    {
        public List<SubEntityId> entities;
        public AssetContainerSetting()
        {
            whitelistPaths = new List<string>();
            blacklistPaths = new List<string>();
            entities = new List<SubEntityId>();
        }
        
        [Serializable]
        internal class SubEntityId
        {
            public string name;
            public string guid;
        }
    }
    
}