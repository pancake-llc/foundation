using System;
using System.Collections.Generic;

namespace PancakeEditor.Attribute
{
    public abstract class TypeProcessor
    {
        public abstract void ProcessType(Type type, List<PropertyDefinition> properties);
    }
}