using System;
using UnityEngine;

namespace Pancake.Database
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.GenericParameter | AttributeTargets.Property)]
    public class EntityDropdownAttribute : PropertyAttribute
    {
        public Type SourceType { get; private set; }
        public EntityDropdownAttribute(Type sourceType) { SourceType = sourceType; }
    }
}