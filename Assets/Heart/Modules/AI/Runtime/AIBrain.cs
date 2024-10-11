#if PANCAKE_AI
using UnityEngine;
using UnityEngine.AI;

namespace Pancake.AI
{
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(Sensor))]
    public class AIBrain : GameComponent
    {
        private NavMeshAgent _agent;
        private Sensor _sensor;

        public NavMeshAgent Agent => _agent;
        public Sensor Sensor => _sensor;

        protected virtual void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            _sensor = GetComponent<Sensor>();
        }
    }
}
#endif