using System;
using System.Collections.Generic;
using UnityEngine;

namespace Pancake.Database
{
    [Serializable]
    public class StaticGroup : IGroup
    {
        public string Title { get => Type.Name; set => Debug.Log($"Tried to set a static group Title to {value}. But, we can'type set the Title on static groups."); }
        public Type Type { get => Type.GetType(typeName); set => typeName = value.AssemblyQualifiedName; }
        [SerializeField, HideInInspector] private string typeName = typeof(Entity).AssemblyQualifiedName;

        public List<Entity> Content { get => datas; set => datas = value; }
        [SerializeField] private List<Entity> datas = new List<Entity>();

        public StaticGroup(Type type) { Type = type; }

        public void Add(Entity data) { Content.Add(data); }

        public void Remove(string key)
        {
            for (var i = 0; i < Content.Count; i++)
            {
                if (Content[i].ID == key) Content.RemoveAt(i);
            }
        }

        public void CleanUp() { Content.RemoveAll(x => x == null); }
    }
}