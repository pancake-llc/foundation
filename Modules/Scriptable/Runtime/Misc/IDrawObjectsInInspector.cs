using System.Collections.Generic;
using UnityEngine;

namespace Pancake.Scriptable
{
    public interface IDrawObjectsInInspector
    {
        List<Object> GetAllObjects();
    }
}