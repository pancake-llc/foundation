using System;
using System.Collections.Generic;
using System.Linq;
#if PANCAKE_ALCHEMY
using Alchemy.Inspector;
using Alchemy.Serialization;
#endif
using Pancake.Scriptable;
using Pancake.Sound;
using UnityEngine;

namespace Pancake.Component
{
    [AlchemySerialize]
    public partial class VfxParticleCollision : GameComponent
    {
        [SerializeField] private ScriptableEventInt updateCoinWithValueEvent;
        [SerializeField] private ScriptableEventNoParam updateCoinEvent;
        [SerializeField] private ListGameObject vfxMagnetCollection;
        [SerializeField] private ScriptableEventGameObject returnPoolEvent;
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

        public void Init(int value)
        {
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
            updateCoinWithValueEvent.Raise(_segmentValue);
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
                returnPoolEvent.Raise(gameObject);
                if (vfxMagnetCollection.Count == 0) updateCoinEvent.Raise();
            }
        }
    }
}