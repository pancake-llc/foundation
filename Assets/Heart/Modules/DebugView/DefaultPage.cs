using DebugUI;
using Pancake.PlayerLoop;
using UnityEngine;

namespace Pancake.DebugView
{
    public class DefaultPage : DebugPageBase
    {
        private short _fps;
        private float _t = 0.1f;

        public override void Configure(DebugUIBuilder builder)
        {
            GameLoop.Register(this, OnFPSUpdated, PlayerLoopTiming.PreUpdate);

            builder.AddField("FPS", () => _fps);

            builder.AddSlider("Time Scale",
                0f,
                3f,
                () => Time.timeScale,
                x => Time.timeScale = x);
            return;
        }

        private void OnFPSUpdated()
        {
            _t += Time.deltaTime;
            if (_t >= 0.1f)
            {
                _t = 0;
                _fps = (short) Mathf.RoundToInt(1f / Time.unscaledDeltaTime);
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            GameLoop.Unregister(this, PlayerLoopTiming.PreUpdate);

        }
    }
}