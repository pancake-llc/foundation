using System;

namespace Pancake.Common
{
    public interface IAsyncProcessHandleSetter
    {
        void Complete(object result);

        void Error(Exception ex);
    }
}