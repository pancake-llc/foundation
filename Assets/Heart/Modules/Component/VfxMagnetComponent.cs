using Sirenix.OdinInspector;
using System.Collections.Generic;
using Pancake.Pools;
using Pancake.Sound;
using UnityEngine;

namespace Pancake.Component
{
    [EditorIcon("icon_default")]
    public class VfxMagnetComponent : GameComponent
    {
        [SerializeField] private StringConstant type;
        [SerializeField] private GameObject fxPrefab;
        [SerializeField] private float fxScale = 1f;
        [SerializeField] private ParticleSystemForceField particleForceField;
        [SerializeField] private bool isPlaySound;

        [ShowIf(nameof(isPlaySound)), Indent, SerializeField, AudioPickup]
        private AudioId audioSpawn;

        private MessageBinding<VfxMagnetMessage> _binding;
        private readonly List<GameObject> _fxInstances = new();

        private void OnEnable()
        {
            _binding ??= new MessageBinding<VfxMagnetMessage>(OnSpawnVfx);
            _binding.Listen = true;
        }

        private void OnDisable() { _binding.Listen = false; }

        private void ReturnFx(GameObject fx)
        {
            _fxInstances.Remove(fx);
            fx.Return();
        }

        private bool IsFxInstanceEmpty() => _fxInstances.Count == 0;

        private void OnSpawnVfx(VfxMagnetMessage msg)
        {
            if (msg.Type != type.Value) return;

            var fx = fxPrefab.Request();
            var vfxParticleCollision = fx.GetComponent<VfxParticleCollision>();
            if (vfxParticleCollision == null) return;
            vfxParticleCollision.Init(msg.Value, ReturnFx, IsFxInstanceEmpty);
            var ps = vfxParticleCollision.PS;
            _fxInstances.Add(fx);
            ps.gameObject.SetActive(true);
            var transformCache = ps.transform;
            transformCache.position = msg.Position;
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
            if (isPlaySound) audioSpawn.Play();
        }
    }
}