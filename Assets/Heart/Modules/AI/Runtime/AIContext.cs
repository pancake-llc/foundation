#if PANCAKE_AI
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Pancake.AI
{
    public class AIContext
    {
        private readonly AIBrain _brain;
        private readonly Dictionary<string, object> _data = new();

        public NavMeshAgent Agent => _brain.Agent;
        public Sensor Sensor => _brain.Sensor;

        public Transform Target { get; set; }

        public AIContext(AIBrain brain) { _brain = brain; }

        public T GetData<T>(string key) => _data.TryGetValue(key, out object result) ? (T) result : default;

        public void SetData(string key, object value) { _data[key] = value; }
    }
}
#endif