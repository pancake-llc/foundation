#if PANCAKE_AI
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Pancake.AI
{
    public class AIContext
    {
        private AIBrain _brain;
        public Transform target;
        public Dictionary<string, object> data = new();

        public NavMeshAgent Agent => _brain.Agent;
        public Sensor Sensor => _brain.Sensor;

        public AIContext(AIBrain brain) { _brain = brain; }

        public T GetData<T>(string key) => data.TryGetValue(key, out object result) ? (T) result : default;

        public void SetData(string key, object value) { data.TryAdd(key, value); }
    }
}
#endif