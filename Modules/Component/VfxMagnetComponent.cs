using System;
using Pancake.Apex;
using Pancake.Scriptable;
using UnityEngine;

namespace Pancake.Component
{
    [HideMonoScript]
    [EditorIcon("csharp")]
    public class VfxMagnetComponent : GameComponent
    {
        [SerializeField] private ScriptableEventVector2 spawnEvent;
        [SerializeField] private GameObjectPool coinFxPool;
        [SerializeField] private float coinFxScale = 1f;
        [SerializeField] private ParticleSystemForceField coinForceField;

        [SerializeField] private Canvas canvas;


        protected override void OnEnabled()
        {
            spawnEvent.OnRaised += SpawnCoinFx;
            coinFxPool.SetParent(canvas.transform, true);
        }

        protected override void OnDisabled()
        {
            spawnEvent.OnRaised -= SpawnCoinFx;
        }

        private void SpawnCoinFx(Vector2 screenPos)
        {
            var coinFx = coinFxPool.Request();
            var ps = coinFx.GetComponent<ParticleSystem>();
            if (ps == null) return;
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
            App.Delay(main.duration / main.simulationSpeed, () => coinFxPool.Return(coinFx));
        }
    }
}