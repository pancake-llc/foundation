using System;
using System.Runtime.CompilerServices;
#if DEV_MODE
using UnityEngine.Profiling;
#endif

namespace Sisus.Shared.EditorOnly
{
    public readonly struct ProfilerScope : IDisposable
    {
        public ProfilerScope([CallerMemberName] string name = null)
        {
#if DEV_MODE
			Profiler.BeginSample(name);
#endif
        }

        public void Dispose()
        {
#if DEV_MODE
			Profiler.EndSample();
#endif
        }
    }
}