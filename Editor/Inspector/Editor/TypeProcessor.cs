using System;
using System.Collections.Generic;

namespace Pancake.Editor
{
    public abstract class TypeProcessor
    {
        public abstract void ProcessType(Type type, List<PropertyDefinition> properties);
    }
}