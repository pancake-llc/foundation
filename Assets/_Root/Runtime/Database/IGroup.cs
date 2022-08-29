using System;
using System.Collections.Generic;

namespace Pancake.Database
{
    public interface IGroup
    {
        public string Title { get; set; }
        public Type Type { get; set; }
        public List<Entity> Content { get; set; }
        public void Add(Entity entity);
        public void Remove(string key);
        public void CleanUp();
    }
}