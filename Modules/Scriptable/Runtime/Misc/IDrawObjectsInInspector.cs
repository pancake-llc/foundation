using System.Collections.Generic;
using UnityEngine;

namespace Obvious.Soap
{
    public interface IDrawObjectsInInspector
    {
        List<Object> GetAllObjects();
    }
}