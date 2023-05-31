using System;
using Pancake.Scriptable;
using UnityEngine;

namespace Pancake.Component
{
    public class VfxMagnetComponent : GameComponent
    {
        [SerializeField] private ScriptableEventVector2 spawnEvent;
        [SerializeField] private GameObjectPool coinFxPool;
        [SerializeField] private float coinFxScale = 1f;
        [SerializeField] private ParticleSystemForceField coinForceField;

        [SerializeField] private Canvas canvas;

        private void Start()
        {
            spawnEvent.OnRaised += SpawnCoinFx;
            coinFxPool.SetParent(canvas.transform, true);
        }

        private void SpawnCoinFx(Vector2 screenPos)
        {
            var ps = coinFxPool.Request().GetComponent<ParticleSystem>();
            if (ps == null) return;
            ps.gameObject.SetActive(true);
            var transformCache = ps.transform;
            transformCache.position = new Vector3(screenPos.x, screenPos.y);
            transformCache.localPosition = new Vector3(transformCache.localPosition.x, transformCache.localPosition.y);
            transformCache.localScale = new Vector3(coinFxScale, coinFxScale, coinFxScale);
            ParticleSystem.ExternalForcesModule externalForcesModule = ps.externalForces;
            externalForcesModule.enabled = true;

            coinForceField.gameObject.SetActive(true);
            externalForcesModule.AddInfluence(coinForceField);
            ps.Play();
        }
    }
}