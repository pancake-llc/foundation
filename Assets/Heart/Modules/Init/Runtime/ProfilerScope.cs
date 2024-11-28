#if DEV_MODE && UNITY_EDITOR
using System;
using System.Runtime.CompilerServices;
using Unity.Profiling;
using UnityEngine.Profiling;

namespace Sisus
{
    public readonly struct ProfilerScope : IDisposable
    {
        public ProfilerScope([CallerMemberName] string name = null) { Profiler.BeginSample(name); }

#if UNITY_6000_0_OR_NEWER
        public ProfilerScope(ProfilerCategory category, [CallerMemberName] string name = null) { Profiler.BeginSample(name); }
#endif

        public void Dispose() { Profiler.EndSample(); }
    }
}
#endif