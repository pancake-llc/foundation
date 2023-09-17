using System;
using Pancake.Apex;
using Pancake.Scriptable;
using Pancake.Sound;
using UnityEngine;

namespace Pancake.Component
{
    [HideMonoScript]
    [EditorIcon("csharp")]
    public class VfxMagnetComponent : GameComponent
    {
        [SerializeField] private ScriptableEventVfxMagnet spawnEvent;
        [SerializeField] private ScriptableListGameObject listVfxMagnetInstance;
        [SerializeField] private GameObjectPool coinFxPool;
        [SerializeField] private ScriptableEventGameObject returnPoolEvent;
        [SerializeField] private float coinFxScale = 1f;
        [SerializeField] private ParticleSystemForceField coinForceField;
        [SerializeField] private bool isPlaySound;
        [SerializeField, ShowIf(nameof(isPlaySound))] private Audio audioSpawn;
        [SerializeField, ShowIf(nameof(isPlaySound))] private ScriptableEventAudio audioPlayEvent;
        [Space] [SerializeField] private bool useCanvasMaster;
        [SerializeField, HideIf(nameof(useCanvasMaster))] private Transform canvas;
        [SerializeField, ShowIf(nameof(useCanvasMaster))] private ScriptableEventGetGameObject getCanvasMasterEvent;


        protected override void OnEnabled()
        {
            spawnEvent.OnRaised += SpawnCoinFx;
            returnPoolEvent.OnRaised += ReturnVfxToPool;

            // trycatch only in editor to avoid case startup from any scene
#if UNITY_EDITOR
            try
            {
#endif
                var parent = useCanvasMaster ? getCanvasMasterEvent.Raise().transform : canvas;
                coinFxPool.SetParent(parent, true);
#if UNITY_EDITOR
            }
            catch (Exception)
            {
                // ignored
            }
#endif
        }

        private void ReturnVfxToPool(GameObject vfx)
        {
            listVfxMagnetInstance.Remove(vfx);
            coinFxPool.Return(vfx);
        }

        protected override void OnDisabled()
        {
            spawnEvent.OnRaised -= SpawnCoinFx;
            returnPoolEvent.OnRaised -= ReturnVfxToPool;
        }

        private void SpawnCoinFx(Vector2 screenPos, int value)
        {
            var coinFx = coinFxPool.Request();
            var vfxParticleCollision = coinFx.GetComponent<VfxParticleCollision>();
            if (vfxParticleCollision == null) return;
            vfxParticleCollision.Init(value);
            var ps = vfxParticleCollision.PS;
            listVfxMagnetInstance.Add(coinFx);
            ps.gameObject.SetActive(true);
            var transformCache = ps.transform;
            transformCache.position = new Vector3(screenPos.x, screenPos.y);
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