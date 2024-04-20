using System;

namespace Pancake.BakingSheet
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Property)]
    public class SheetValueConverterAttribute : Attribute
    {
        public Type ConverterType { get; }

        public SheetValueConverterAttribute(Type converterType) { ConverterType = converterType; }
    }
}