#if PANCAKE_AI
using System.Collections.Generic;
using UnityEngine;

namespace Pancake.AI
{
    public class AIContext
    {
        public AIBrain brain;
        public Transform target;

        public Dictionary<string, object> data = new();

        public AIContext(AIBrain brain) { this.brain = brain; }

        public object GetData<T>(string key) => data.TryGetValue(key, out object result) ? (T) result : default;

        public void SetData(string key, object value) { data.TryAdd(key, value); }
    }
}
#endif