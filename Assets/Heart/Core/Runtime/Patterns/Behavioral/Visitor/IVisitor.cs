using UnityEngine;

namespace Pancake.Pattern
{
    public interface IVisitor
    {
        void Visit<T>(T visitable) where T : Component, IVisitable;
    }
}