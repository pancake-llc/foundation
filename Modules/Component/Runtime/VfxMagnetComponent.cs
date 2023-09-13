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
        [SerializeField] private float coinFxScale = 1f;
        [SerializeField] private ParticleSystemForceField coinForceField;
        [SerializeField] private bool isPlaySound;
        [SerializeField, ShowIf(nameof(isPlaySound))] private Audio audioSpawn;
        [SerializeField, ShowIf(nameof(isPlaySound))] private ScriptableEventAudio audioPlayEvent;
        [Space] [SerializeField] private bool useCanvasMaster;
        [SerializeField, HideIf(nameof(useCanvasMaster))] private Canvas canvas;
        [SerializeField, ShowIf(nameof(useCanvasMaster))] private ScriptableEventGetGameObject getCanvasMasterEvent;


        protected override void OnEnabled()
        {
            spawnEvent.OnRaised += SpawnCoinFx;

            // trycatch only in editor to avoid case startup from any scene
#if UNITY_EDITOR
            try
            {
#endif
                var parent = useCanvasMaster ? getCanvasMasterEvent.Raise().transform : canvas.transform;
                coinFxPool.SetParent(parent, true);
#if UNITY_EDITOR
            }
            catch (Exception)
            {
                // ignored
            }
#endif
        }

        protected override void OnDisabled() { spawnEvent.OnRaised -= SpawnCoinFx; }

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
            ps.Play();
            var main = ps.main;
            if (isPlaySound) audioPlayEvent.Raise(audioSpawn);
            
            App.Delay(main.duration / main.simulationSpeed,
                () =>
                {
                    listVfxMagnetInstance.Remove(coinFx);
                    coinFxPool.Return(coinFx);
                });
        }
    }
}