using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Pancake.Linq;

namespace Pancake.Database
{
    [CreateAssetMenu(fileName = "Global.Database.asset", menuName = "Pancake/Create Database", order = 0)]
    // ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
    public class Database : ScriptableObject, ISerializationCallbackReceiver
    {
        public const string GLOBAL_DATABASE_NAME = "Global.Database";
        public Dictionary<string, Entity> data = new Dictionary<string, Entity>(); // no boxing required

        [SerializeField] private List<string> keys = new List<string>();
        [SerializeField] private List<Entity> values = new List<Entity>();
        [SerializeField] private List<StaticGroup> staticGroups = new List<StaticGroup>();
        
        /// <summary>
        /// Callback which will get the move data from the Dictionary (which cannot serialize) to the key/value Lists (which can be serialized)
        /// </summary>
        public virtual void OnBeforeSerialize()
        {
            keys.Clear();
            values.Clear();
            foreach (var d in data)
            {
                keys.Add(d.Key);
                values.Add(d.Value);
            }
        }

        /// <summary>
        /// Callback which will move the data from the key/value Lists (which have been serialized) and create the Dictionaries (which can't serialize).
        /// </summary>
        public void OnAfterDeserialize()
        {
            // Standard Data
            data = new Dictionary<string, Entity>();
            for (var i = 0; i < keys.Count; i++)
            {
                data.Add(keys[i], values[i]);
            }
        }

        /// <summary>
        /// Query a Entity from the Database. Uses a dictionary lookup which is 0(1).
        /// </summary>
        /// <param name="key">The unique key for the Entity.</param>
        /// <returns>The Entity reference, or Null if the key doesn't exist in the Database.</returns>
        public Entity Query(string key) { return data.ContainsKey(key) ? data[key] : null; }
        
        /// <summary>
        /// Query all DataEntities that inherit from a specific type. This is a linear search that will cast every entry in the DB to type T. Very slow, cache your results and avoid frequent use.
        /// </summary>
        /// <typeparam name="T">The Type you want collected. Must derive from Entity.</typeparam>
        /// <returns>A list of all DataEntities in the database that can successfully cast to T</returns>
        public List<T> Query<T>() { return values.OfType<T>().ToList().Map(x => (T) Convert.ChangeType(x, typeof(T))); }

        /// <summary>                                                                                                                                                       
        /// Add an item to the Database. Using this at runtime is not really useful since it doesn't persist between sessions, but maybe so if you are hotloading in custom content at runtime every session.
        /// </summary>
        /// <param name="data">The data reference.</param>
        /// <param name="generateNewId">If true, a new ID will be generated for the item.</param>
        public virtual void Add(Entity data, bool generateNewId = true)
        {
            string id = generateNewId ? Ulid.NewUlid().ToString() : data.ID;
            if (this.data.ContainsKey(id)) return;
            data.ID = id;
            this.data.Add(id, data);
        }

        /// <summary>
        /// Remove an item from the Database. Using this at runtime is not really useful since it doesn't persist between sessions, but maybe so if you are hotloading in custom content at runtime every session.
        /// </summary>
        /// <param name="data">The data reference.</param>
        public virtual void Remove(Entity data)
        {
            if (this.data.ContainsKey(data.ID))
            {
                this.data.Remove(data.ID);
            }
        }

        /// <summary>
        /// Remove an item from the Database. This is a fast O(1) operation. Using this at runtime is not really useful since it doesn't persist between sessions, but maybe so if you are hotloading in custom content at runtime every session.
        /// </summary>
        /// <param name="id">The unique key ID for the item.</param>
        public virtual void Remove(string id)
        {
            if (data.ContainsKey(id))
            {
                data.Remove(id);
            }
        }

        /// <summary>
        /// Query the group responsible for the type T.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public virtual StaticGroup GetStaticGroup<T>() where T : Entity { return staticGroups.Single(x => x.Type == typeof(T)); }

        /// <summary>
        /// Query the group responsible for the type T.
        /// </summary>
        /// <returns></returns>
        public virtual StaticGroup GetStaticGroup(Type t) { return staticGroups.Single(x => x.Type == t); }

        public virtual List<StaticGroup> GetAllStaticGroups() { return staticGroups; }

        /// <summary>
        /// Set the group responsible for the given type.
        /// </summary>
        /// <param name="identifier">The class itself.</param>
        public virtual void SetStaticGroup(StaticGroup identifier)
        {
            foreach (var x in staticGroups.Filter(x => x.Type == identifier.Type))
            {
                staticGroups.Remove(identifier);
            }

            staticGroups.Add(identifier);
        }

        /// <summary>
        /// Clear the entire database of all Data content.
        /// </summary>
        public virtual void ClearData()
        {
            data.Clear();
            keys.Clear();
            values.Clear();
        }

        /// <summary>
        /// Clear the entire database of all Group content.
        /// </summary>
        public virtual void ClearStaticGroups() { staticGroups.Clear(); }

        /// <summary>
        /// Query the number of Entities in the Database. Does not include groups.
        /// </summary>
        /// <returns></returns>
        public virtual int GetEntityCount() { return data.Count; }
    }
}