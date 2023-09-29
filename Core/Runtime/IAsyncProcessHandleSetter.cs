using System;

namespace Pancake
{
    internal interface IAsyncProcessHandleSetter
    {
        void Complete(object result);

        void Error(Exception ex);
    }
}