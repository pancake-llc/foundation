using System;
using System.Reflection;

namespace Pancake.BakingSheet
{
    public abstract class SheetVerifier
    {
        public abstract bool CanVerify(PropertyInfo propertyInfo, Type type);

        public abstract string Verify(PropertyInfo propertyInfo, object value);
    }

    public abstract class SheetVerifier<TValue> : SheetVerifier
    {
        public sealed override bool CanVerify(PropertyInfo propertyInfo, Type type) { return typeof(TValue).IsAssignableFrom(type); }

        public sealed override string Verify(PropertyInfo propertyInfo, object value) { return Verify(propertyInfo, (TValue) value); }

        public abstract string Verify(PropertyInfo propertyInfo, TValue value);
    }
}