#if PANCAKE_ALCHEMY
using Alchemy.Inspector;
#endif
using System.Collections.Generic;
using Pancake.Sound;
using UnityEngine;

namespace Pancake.Component
{
    public struct VfxMangnetEvent : IEvent
    {
        public string type;
        public Vector3 position;
        public int value;
    }

    [EditorIcon("icon_default")]
    public class VfxMagnetComponent : GameComponent
    {
        [SerializeField] private StringConstant type;
        [SerializeField] private GameObject fxPrefab;
        [SerializeField] private float fxScale = 1f;
        [SerializeField] private ParticleSystemForceField particleForceField;
        [SerializeField] private bool isPlaySound;
#if PANCAKE_ALCHEMY
        [ShowIf(nameof(isPlaySound)), Indent]
#endif
        [SerializeField]
        private Audio audioSpawn;

        private readonly List<GameObject> _fxInstances = new();
        private EventBinding<VfxMangnetEvent> _binding;

        private void Awake() { _binding = new EventBinding<VfxMangnetEvent>(OnSpawnVfx); }

        protected void OnEnable() { _binding.Listen = true; }

        protected void OnDisable() { _binding.Listen = false; }

        private void ReturnFx(GameObject fx)
        {
            _fxInstances.Remove(fx);
            fx.Return();
        }

        private bool IsFxInstanceEmpty() => _fxInstances.Count == 0;

        private void OnSpawnVfx(VfxMangnetEvent data)
        {
            if (data.type != type.Value) return;

            var fx = fxPrefab.Request();
            var vfxParticleCollision = fx.GetComponent<VfxParticleCollision>();
            if (vfxParticleCollision == null) return;
            vfxParticleCollision.Init(data.value, ReturnFx, IsFxInstanceEmpty);
            var ps = vfxParticleCollision.PS;
            _fxInstances.Add(fx);
            ps.gameObject.SetActive(true);
            var transformCache = ps.transform;
            transformCache.position = data.position;
            var localPos = transformCache.localPosition;
            localPos = new Vector3(localPos.x, localPos.y);
            transformCache.localPosition = localPos;
            transformCache.localScale = new Vector3(fxScale, fxScale, fxScale);
            var externalForcesModule = ps.externalForces;
            externalForcesModule.enabled = true;
            particleForceField.gameObject.SetActive(true);
            externalForcesModule.AddInfluence(particleForceField);
            ps.Emit(1); // avoid zero particle count when start
            ps.Play();
            if (isPlaySound && audioSpawn != null) audioSpawn.PlaySfx();
        }
    }
}