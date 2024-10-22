using Pancake.AI;
using Pancake.Game.Interfaces;
using Sisus.Init;
using UnityEngine;

namespace Pancake.Game
{
    public class Player : MonoBehaviour<IPlayerStat>
    {
        [SerializeField] private StringConstant contextDataKey;
        private IPlayerStat _stat;
        private AIBrain _brain;

        public AIBrain Brain
        {
            get
            {
                if (_brain == null) _brain = GetComponent<AIBrain>();
                return _brain;
            }
        }

        protected override void Init(IPlayerStat argument) { _stat = argument; }

        private void Start()
        {
            _stat.Health = _stat.MaxHealth;
            _brain = GetComponent<AIBrain>();
        }

        private void OnEnable() { Brain.updateContext += UpdateContext; }

        private void UpdateContext(AIContext context)
        {
            float normalized = _stat.Health / (float) _stat.MaxHealth;
            context.SetData(contextDataKey.Value, normalized);
        }

        private void OnDisable() { Brain.updateContext -= UpdateContext; }

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out ICollectable collectable)) collectable.Collect();
        }
    }
}