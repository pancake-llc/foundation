using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pancake.Editor.SOA
{
    public interface IPropertyIterator
    {
        bool Next();
        void End();
    }

}