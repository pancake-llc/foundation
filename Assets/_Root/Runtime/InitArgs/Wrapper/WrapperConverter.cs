using System;
using System.ComponentModel;
using System.Globalization;

namespace Pancake.Init
{
    internal sealed class WrapperConverter : TypeConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return !(context is null) && context.Instance is IWrapper wrapper && wrapper.WrappedObject != null
                ? destinationType.IsAssignableFrom(wrapper.WrappedObject.GetType())
                : base.CanConvertTo(context, destinationType);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            return value is IWrapper wrapper && wrapper.WrappedObject is object wrapped && destinationType.IsAssignableFrom(wrapped.GetType())
                ? wrapped
                : null;
        }
    }
}