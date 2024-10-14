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
        public List<AIAction> actions;
        public AIContext context;

        public Action<AIContext> updateContext;

        private NavMeshAgent _agent;
        private Sensor _sensor;

        public NavMeshAgent Agent => _agent;
        public Sensor Sensor => _sensor;

        protected virtual void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            _sensor = GetComponent<Sensor>();
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