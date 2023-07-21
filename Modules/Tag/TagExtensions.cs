using System.Collections.Generic;
using Pancake.Scriptable;
using UnityEngine;

namespace Pancake.ExTag
{
    public static class TagExtensions
    {
        /// <summary>
        /// Retrieves all tags for a given `GameObject`. A faster alternative to `gameObject.GetComponen&lt;UATags&gt;().Tags`.
        /// </summary>
        /// <param name="go">This `GameObject`</param>
        /// <returns>
        /// A `ReadOnlyList&lt;T&gt;` of tags stored as `StringContant`s. Returns `null` if the `GameObject` does not have any tags or if the `GameObject` is disabled.
        /// </returns>
        public static ReadOnlyList<StringConstant> GetTags(this GameObject go) { return Tag.GetTags(go); }

        /// <summary>
        /// Check if the tag provided is associated with this `GameObject`.
        /// </summary>
        /// <param name="go">This `GameObject`</param>
        /// <param name="tag">The tag to search for.</param>
        /// <returns>`true` if the tag exists, otherwise `false`.</returns>
        public static bool HasTag(this GameObject go, string tag)
        {
            var tags = Tag.GetTagsForGameObject(go);
            if (tags == null) return false;
            return tags.HasTag(tag);
        }

        /// <summary>
        /// Check if the tag provided is associated with this `GameObject`.
        /// </summary>
        /// <param name="go">This `GameObject`</param>
        /// <param name="tag">The tag to search for.</param>
        /// <returns>`true` if the tag exists, otherwise `false`.</returns>
        public static bool HasTag(this GameObject go, StringConstant tag) { return go.HasTag(tag.Value); }

        /// <summary>
        /// Check if any of the tags provided are associated with this `GameObject`.
        /// </summary>
        /// <param name="go">This `GameObject`</param>
        /// <param name="tags">The tags to search for.</param>
        /// <returns>`true` if any of the tags exist, otherwise `false`.</returns>
        public static bool HasAnyTag(this GameObject go, List<string> tags)
        {
            var goTags = Tag.GetTagsForGameObject(go);
            if (goTags == null) return false;

            for (var i = 0; i < tags.Count; i++)
            {
                if (goTags.HasTag(tags[i])) return true;
            }

            return false;
        }

        /// <summary>
        /// Check if any of the tags provided are associated with this `GameObject`.
        /// </summary>
        /// <param name="go">This `GameObject`</param>
        /// <param name="stringConstants">The tags to search for.</param>
        /// <returns>`true` if any of the tags exist, otherwise `false`.</returns>
        public static bool HasAnyTag(this GameObject go, List<StringConstant> stringConstants)
        {
            // basically same method as above, the code is mostly copy and pasted because its not preferable to convert
            // stringconstants to strings and calling the other method, because of memory allocation
            var tags = Tag.GetTagsForGameObject(go);
            if (tags == null) return false;

            for (var i = 0; i < stringConstants.Count; i++)
            {
                if (tags.HasTag(stringConstants[i].Value)) return true;
            }

            return false;
        }
        
        /// <summary>
        /// Remove
        /// </summary>
        /// <param name="go"></param>
        /// <param name="tag"></param>
        public static void RemoveTag(this GameObject go, string tag)
        {
            var tags = Tag.GetTagsForGameObject(go);
            if (tags == null) return; 
            
            tags.RemoveTag(tag);
        }

        /// <summary>
        /// Add tag for gameobject
        /// </summary>
        /// <param name="go"></param>
        /// <param name="tag"></param>
        public static void AddTag(this GameObject go, StringConstant tag)
        {
            var tags = Tag.GetTagsForGameObject(go); 
            if (tags == null) return; 
            tags.AddTag(tag);
        }
    }
}