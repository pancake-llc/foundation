namespace Pancake.Sound
{
    public static class FlagsExtension
    {
        public enum FlagsRangeType
        {
            Included,
            Excluded,
        }

        public static int GetFlagsOnCount(int flags)
        {
            var count = 0;
            while (flags != 0)
            {
                flags &= (flags - 1);
                count++;
                if (count > 32) // integer has only 32-bit max
                {
                    UnityEngine.Debug.LogError("count flags is failed");
                    break;
                }
            }

            return count;
        }

        public static int GetFlagsRange(int minIndex, int maxIndex, FlagsRangeType rangeType)
        {
            var flagsRange = 0;
            for (int i = minIndex; i <= maxIndex; i++)
            {
                flagsRange += 1 << i;
            }

            switch (rangeType)
            {
                case FlagsRangeType.Included: return flagsRange;
                case FlagsRangeType.Excluded: return ~flagsRange;
                default: return default;
            }
        }
    }
}