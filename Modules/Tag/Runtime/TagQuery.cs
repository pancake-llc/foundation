using UnityEngine;

namespace Pancake.Tag
{
    [CreateAssetMenu(fileName = "TagQuery", menuName = "Pancake/Tag/Tag Query")]
    public class TagQuery : ScriptableTag
    {
        public TagStatic.TagWithRule[] matchingTags;
        public TagStatic.TagQueryWithTarget[] subQueries;
    }
}