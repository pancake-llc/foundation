using System;
using System.Collections.Generic;

namespace Pancake.Editor.LevelEditor
{
    [Serializable]
    internal class Settings
    {
        public List<string> pickupObjectWhiteList;
        public List<string> pickupObjectBlackList;

        public Settings()
        {
            pickupObjectBlackList = new List<string>();
            pickupObjectWhiteList = new List<string>();
        }
    }
}