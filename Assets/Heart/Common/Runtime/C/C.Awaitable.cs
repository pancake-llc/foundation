using System;
using System.Threading;


namespace Pancake.Common
{
    public static partial class C
    {
        public static async UnityEngine.Awaitable WaitUntil(Func<bool> condition, CancellationToken cancellationToken = default)
        {
            while (!condition())
                await UnityEngine.Awaitable.EndOfFrameAsync(cancellationToken);
        }
    }
}