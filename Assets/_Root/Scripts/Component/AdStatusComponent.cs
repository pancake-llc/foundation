using System;
using System.Collections;
using Pancake.Common;
using Pancake.Monetization;
using UnityEngine;

namespace Pancake.SceneFlow
{
    public class AdStatusComponent : GameComponent
    {
        [SerializeField] private GameObject fetch;

        private readonly WaitForSeconds _wait = new WaitForSeconds(0.1f);
        private AsyncProcessHandle _handle;

        protected void OnEnable()
        {
            if (!Application.isMobilePlatform)
            {
                fetch.SetActive(false);
                return;
            }

            _handle = App.StartCoroutine(IeValidate());
        }

        protected void OnDisable() { App.StopAndClean(ref _handle); }

        private IEnumerator IeValidate()
        {
            while (true)
            {
                fetch.SetActive(!Advertising.Reward.IsReady());
                yield return _wait;
            }
            // ReSharper disable once IteratorNeverReturns
        }
    }
}