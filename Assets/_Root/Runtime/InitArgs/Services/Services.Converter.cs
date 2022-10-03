using System;
using System.ComponentModel;
using System.Globalization;

namespace Pancake.Init.Internal
{
    [TypeConverter(typeof(Converter))]
    public partial class Services
    {
        private sealed class Converter : TypeConverter
        {
            public override bool CanConvertFrom(ITypeDescriptorContext context, Type destinationType) => false;

            public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType) => true;

            public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
            {
                if(value is Services services)
                {
                    foreach(var service in services.providesServices)
                    {
                        if(destinationType == service.definingType.Value)
                        {
                            return service.service;
                        }
                    }
                }

                return base.ConvertTo(context, culture, value, destinationType);
            }
        }
    }
}