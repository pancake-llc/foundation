using System;
using UnityEngine;

namespace Pancake.Database
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.GenericParameter | AttributeTargets.Property)]
    public class SpritePreviewAttribute : PropertyAttribute
    {
        public SpritePreviewAttribute() { }
    }
}