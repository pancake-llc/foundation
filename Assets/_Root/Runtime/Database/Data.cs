using System.Collections.Generic;
using UnityEngine;

namespace Pancake.Database
{
    public static class Data
    {
        private static Database database;

        /// <summary>
        /// All of the references to project assets. Not recommended to access directly.
        /// Use the Data class methods or create your own extension methods, such as non-linear search types.
        /// </summary>
        public static Database Database
        {
            get
            {
                if (database == null) Find();
                return database;
            }
            set => database = value;
        }

        /// <summary>
        /// Directly query the database for a specific id. This is the most efficient way to access data.
        /// </summary>
        /// <param name="id">The item ID</param>
        /// <returns>A reference to the <see cref="Entity"/>.</returns>
        public static Entity Query(string id) { return Database.Query(id); }

        /// <summary>
        /// Directly query the database for a specific id. This is the most efficient way to access data.
        /// </summary>
        /// <param name="id">The item ID</param>
        /// <returns>A reference to the <see cref="Entity"/>.</returns>
        public static Entity Q(string id) { return Database.Query(id); }

        /// <summary>
        /// Slow way to get every item of a specific type.
        /// </summary>
        /// <typeparam name="T">The type of Items you want.</typeparam>
        /// <returns>All of the items that are of the given type.</returns>
        public static List<T> Query<T>() where T : Entity { return Database.Query<T>(); }

        /// <summary>
        /// Slow way to get every item of a specific type.
        /// </summary>
        /// <typeparam name="T">The type of Items you want.</typeparam>
        /// <returns>All of the items that are of the given type.</returns>
        public static List<T> Q<T>() where T : Entity { return Database.Query<T>(); }

        private static void Find()
        {
            database = (Database) Resources.Load(Database.GLOBAL_DATABASE_NAME);
            if (database == null) Debug.LogWarning($"Can not find :{Database.GLOBAL_DATABASE_NAME}");
        }
    }
}