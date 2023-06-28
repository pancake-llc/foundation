using System.Collections.Generic;
using Pancake.BTag;
using UnityEditor;
using UnityEngine;

namespace Pancake.BTagEditor
{
    public class TagEditorUtils : BTagEditorUtils<TagGroup, Tag>
    {
        public static Tag CreateTag(string groupPath, string tagName, bool createIfAlreadyExists = false) => Create(groupPath, tagName, createIfAlreadyExists);
        public static Tag CreateTag(TagGroup tagGroup, string tagName, bool createIfAlreadyExists = false) => Create(tagGroup, tagName, createIfAlreadyExists);
        public static List<Tag> GetTagsForGroup(TagGroup tagGroup) => GetSOsForGroup(tagGroup);

        public static TagGameObject TagGameObject(GameObject gameObject, TagGroup tagGroup, string tagName, bool createTagIfNotFound = true)
        {
            Tag tag = default;
            if (tagGroup == null) tagGroup = AssetDatabase.LoadMainAssetAtPath("Assets/NewGroup.asset") as TagGroup;
            if (tagGroup != null)
            {
                var potentialTags = AssetDatabase.LoadAllAssetRepresentationsAtPath(AssetDatabase.GetAssetPath(tagGroup));
                for (int i = 0; i < potentialTags.Length; ++i)
                {
                    if (potentialTags[i] is Tag && potentialTags[i].name.ToLower() == tagName.ToLower())
                    {
                        tag = potentialTags[i] as Tag;
                        break;
                    }
                }
            }

            if (tag == default)
            {
                if (!createTagIfNotFound) return default;
                tag = CreateTag(tagGroup, tagName);
            }

            return TagGameObject(gameObject, tag);
        }

        public static TagGameObject TagGameObject(GameObject gameObject, Tag tag)
        {
            var existing = gameObject.GetComponents<TagGameObject>();
            for (int i = 0; i < existing.Length; ++i)
            {
                if (existing[i].tag == tag) return existing[i];
            }

            var tgo = gameObject.AddComponent<TagGameObject>();
            Undo.RegisterCreatedObjectUndo(tgo, "Tag " + gameObject);
            tgo.tag = tag;
            return tgo;
        }
    }
}