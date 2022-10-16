using System;
using System.Collections.Generic;

namespace Pancake
{
    public interface ITempCollection<T> : ICollection<T>, IDisposable
    {
    }
}