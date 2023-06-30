using System.Collections.Generic;
using Pancake.Tag;
using UnityEditor;
using UnityEngine;

namespace Pancake.TagEditor
{
    public class TagEditorUtilExtend : TagEditorUtils<ScriptableTagGroup, Tag.Tag>
    {
        public static Tag.Tag CreateTag(string groupPath, string tagName, bool createIfAlreadyExists = false) => Create(groupPath, tagName, createIfAlreadyExists);
        public static Tag.Tag CreateTag(ScriptableTagGroup scriptableTagGroup, string tagName, bool createIfAlreadyExists = false) => Create(scriptableTagGroup, tagName, createIfAlreadyExists);
        public static List<Tag.Tag> GetTagsForGroup(ScriptableTagGroup scriptableTagGroup) => GetSOsForGroup(scriptableTagGroup);

        public static TagGameObject TagGameObject(GameObject gameObject, ScriptableTagGroup scriptableTagGroup, string tagName, bool createTagIfNotFound = true)
        {
            Tag.Tag tag = default;
            if (scriptableTagGroup == null) scriptableTagGroup = AssetDatabase.LoadMainAssetAtPath("Assets/NewGroup.asset") as ScriptableTagGroup;
            if (scriptableTagGroup != null)
            {
                var potentialTags = AssetDatabase.LoadAllAssetRepresentationsAtPath(AssetDatabase.GetAssetPath(scriptableTagGroup));
                for (int i = 0; i < potentialTags.Length; ++i)
                {
                    if (potentialTags[i] is Tag.Tag && potentialTags[i].name.ToLower() == tagName.ToLower())
                    {
                        tag = potentialTags[i] as Tag.Tag;
                        break;
                    }
                }
            }

            if (tag == default)
            {
                if (!createTagIfNotFound) return default;
                tag = CreateTag(scriptableTagGroup, tagName);
            }

            return TagGameObject(gameObject, tag);
        }

        public static TagGameObject TagGameObject(GameObject gameObject, Tag.Tag tag)
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