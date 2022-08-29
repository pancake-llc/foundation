using System;
using System.Collections.Generic;
using UnityEngine;

namespace Pancake.Database
{
    // ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
    public class CustomGroup : Entity, IGroup
    {
        public Type Type { get => Type.GetType(typeName); set => typeName = value.AssemblyQualifiedName; }
        [SerializeField] [HideInInspector] private string typeName = typeof(Entity).AssemblyQualifiedName;

        public List<Entity> Content { get => datas; set => datas = value; }
        [SerializeField] private List<Entity> datas = new List<Entity>();

        protected override void Reset()
        {
            base.Reset();
            Title = "Custom Group";
            Description = "Used to store a list of custom Data Entity types for easy reference.";
        }

        public virtual void Add(Entity entity)
        {
            if (Content.Contains(entity)) return;
            Content.Add(entity);
            EditorHandleDirty();
        }

        public virtual void Remove(string key)
        {
            for (int i = 0; i < Content.Count; i++)
            {
                if (Content[i].ID == key)
                {
                    Content.RemoveAt(i);
                }
            }

            EditorHandleDirty();
        }

        public virtual void CleanUp() { Content.RemoveAll(x => x == null); }

        protected virtual void EditorHandleDirty()
        {
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();
#endif
        }
    }
}