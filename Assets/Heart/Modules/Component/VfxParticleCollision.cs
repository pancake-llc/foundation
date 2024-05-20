using System;
using System.Collections.Generic;
using System.Linq;
#if PANCAKE_ALCHEMY
using Alchemy.Inspector;
using Alchemy.Serialization;
#endif
using Pancake.Sound;
using UnityEngine;
using VitalRouter;

namespace Pancake.Component
{
    [AlchemySerialize]
    public partial class VfxParticleCollision : GameComponent
    {
        public StringConstant type;
        [field: SerializeField] public ParticleSystem PS { get; private set; }
#if PANCAKE_ALCHEMY
        [AlchemySerializeField, NonSerialized]
#endif
        public Dictionary<int, int> numberParticleMap = new();

        [SerializeField] private bool enabledSound;
#if PANCAKE_ALCHEMY
        [ShowIf(nameof(enabledSound))]
#endif
        [SerializeField]
        private Audio audioCollision;

        private int _segmentValue;
        private bool _flag;
        private Action<GameObject> _returnEvent;
        private Func<bool> _isFxInstanceEmpty;

        public void Init(int value, Action<GameObject> returnEvent, Func<bool> isFxInstanceEmpty)
        {
            _returnEvent = returnEvent;
            _isFxInstanceEmpty = isFxInstanceEmpty;
            _flag = false;

            var sorted = numberParticleMap.OrderByDescending(x => x.Key).ToList();
            int maxParticle = sorted.First().Value;
            foreach (var particle in sorted)
            {
                if (value >= particle.Key)
                {
                    maxParticle = particle.Value;
                    break;
                }
            }

            var main = PS.main;
            main.maxParticles = maxParticle;
            _segmentValue = value / maxParticle;
        }

        private void OnParticleCollision(GameObject particle)
        {
            Router.Default.PublishAsync(new UpdateCurrencyWithValueCommand(type.Value, _segmentValue));
            if (enabledSound && audioCollision != null) audioCollision.PlaySfx();
        }

        protected void Update()
        {
            if (PS.particleCount > 0) return;

            if (!_flag)
            {
                _flag = true;
                // remove external force module
                ParticleSystem.ExternalForcesModule externalForcesModule = PS.externalForces;
                externalForcesModule.RemoveAllInfluences();
                externalForcesModule.enabled = false;
                _returnEvent.Invoke(gameObject);
                if (_isFxInstanceEmpty.Invoke()) Router.Default.PublishAsync(new UpdateCurrencyCommand(type.Value));
            }
        }
    }
}