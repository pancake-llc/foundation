using System;

namespace Pancake.LevelSystem
{
    [Serializable]
    public class LevelNode
    {
        public int level;
        public LevelGameObject[] objects;
        public ExtraInfo[] extraInfos;
    }
}