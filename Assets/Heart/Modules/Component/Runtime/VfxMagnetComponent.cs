#if PANCAKE_ALCHEMY
using Alchemy.Inspector;
#endif
using Pancake.Scriptable;
using Pancake.Sound;
using UnityEngine;

namespace Pancake.Component
{
    [EditorIcon("icon_default")]
    public class VfxMagnetComponent : GameComponent
    {
        [SerializeField] private ScriptableEventVfxMagnet spawnEvent;
        [SerializeField] private ListGameObject listVfxMagnetInstance;
        [SerializeField] private GameObject coinFxPrefab;
        [SerializeField] private ScriptableEventGameObject returnPoolEvent;
        [SerializeField] private float coinFxScale = 1f;
        [SerializeField] private ParticleSystemForceField coinForceField;
        [SerializeField] private bool isPlaySound;
#if PANCAKE_ALCHEMY
        [ShowIf(nameof(isPlaySound)), Indent]
#endif
        [SerializeField]
        private Audio audioSpawn;

#if PANCAKE_ALCHEMY
        [ShowIf(nameof(isPlaySound)), Indent]
#endif
        [SerializeField]
        private ScriptableEventAudio audioPlayEvent;

        protected void OnEnable()
        {
            spawnEvent.OnRaised += SpawnCoinFx;
            returnPoolEvent.OnRaised += ReturnVfxToPool;
        }

        private void ReturnVfxToPool(GameObject vfx)
        {
            listVfxMagnetInstance.Remove(vfx);
            vfx.Return();
        }

        protected void OnDisable()
        {
            spawnEvent.OnRaised -= SpawnCoinFx;
            returnPoolEvent.OnRaised -= ReturnVfxToPool;
        }

        private void SpawnCoinFx(Vector3 position, int value)
        {
            var coinFx = coinFxPrefab.Request();
            var vfxParticleCollision = coinFx.GetComponent<VfxParticleCollision>();
            if (vfxParticleCollision == null) return;
            vfxParticleCollision.Init(value);
            var ps = vfxParticleCollision.PS;
            listVfxMagnetInstance.Add(coinFx);
            ps.gameObject.SetActive(true);
            var transformCache = ps.transform;
            transformCache.position = position;
            var localPos = transformCache.localPosition;
            localPos = new Vector3(localPos.x, localPos.y);
            transformCache.localPosition = localPos;
            transformCache.localScale = new Vector3(coinFxScale, coinFxScale, coinFxScale);
            ParticleSystem.ExternalForcesModule externalForcesModule = ps.externalForces;
            externalForcesModule.enabled = true;

            coinForceField.gameObject.SetActive(true);
            externalForcesModule.AddInfluence(coinForceField);
            ps.Emit(1); // avoid zero particle count when start
            ps.Play();
            if (isPlaySound) audioPlayEvent.Raise(audioSpawn);
        }
    }
}