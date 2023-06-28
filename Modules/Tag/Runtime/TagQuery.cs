using UnityEngine;

namespace Pancake.BTag
{
    [CreateAssetMenu(fileName = "TagQuery", menuName = "Pancake/BTag/Tag Query")]
    public class TagQuery : ScriptableBTag
    {
        public BTag.TagWithRule[] matchingTags;
        public BTag.TagQueryWithTarget[] subQueries;
    }
}