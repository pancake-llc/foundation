#if UNITY_EDITOR
using System;
using UnityEngine;

namespace Sisus.ComponentNames.Editor
{
    /// <summary>
    /// A non-generic interface implemented by <see cref="CustomHeader{TComponent}"/>.
    /// </summary>
    internal interface ICustomHeader : IComparable<ICustomHeader>
    {
        int ExecutionOrder { get; }
        void Init(Name name, Suffix suffix, Tooltip tooltip, Name defaultName, Suffix defaultSuffix, Tooltip defaultTooltip);
        Name GetName(Component target);
        Suffix GetSuffix(Component target);
        Tooltip GetTooltip(Component target);
    }
}
#endif