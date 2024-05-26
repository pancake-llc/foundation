using System;
using DebugUI;
using R3;
using UnityEngine;
using static System.TimeSpan;

namespace Pancake.DebugView
{
    public class DefaultPage : DebugPageBase
    {
        protected override string Label => "";

        private short _fps;

        public override void Configure(DebugUIBuilder builder)
        {
            Observable.Interval(FromSeconds(0.1f)).Subscribe(OnFPSUpdated);

            builder.AddField("FPS", () => _fps);

            builder.AddSlider("Time Scale",
                0f,
                3f,
                () => Time.timeScale,
                x => Time.timeScale = x);
            return;
            void OnFPSUpdated(Unit _) { _fps = (short) (Mathf.RoundToInt(1f / Time.unscaledDeltaTime)); }
        }
    }
}