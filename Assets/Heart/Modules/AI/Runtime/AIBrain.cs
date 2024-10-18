#if PANCAKE_AI
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Pancake.AI
{
    [RequireComponent(typeof(NavMeshAgent))]
    [EditorIcon("icon_controller")]
    public class AIBrain : GameComponent
    {
        [SerializeField] private Optional<NavMeshAgent> agent = new(false, null);
        [SerializeField] private Optional<Sensor> sensor = new(false, null);

        public List<AIAction> actions;
        public AIContext context;

        public Action<AIContext> updateContext;

        public NavMeshAgent Agent => agent.Value;
        public Sensor Sensor => sensor.Value;

        protected virtual void Awake()
        {
            if (Agent == null) agent = new Optional<NavMeshAgent>(false, GetComponent<NavMeshAgent>());
            if (Sensor == null) sensor = new Optional<Sensor>(false, GetComponent<Sensor>());
            context = new AIContext(this);

            foreach (var action in actions)
            {
                action.Initialize(context);
            }
        }

        private void Update()
        {
            updateContext?.Invoke(context);

            AIAction bestAction = null;
            var highestPriority = float.MinValue;

            foreach (var action in actions)
            {
                float priority = action.CalculatePriority(context);
                if (priority > highestPriority)
                {
                    highestPriority = priority;
                    bestAction = action;
                }
            }

            bestAction?.Execute(context);
        }
    }
}
#endif