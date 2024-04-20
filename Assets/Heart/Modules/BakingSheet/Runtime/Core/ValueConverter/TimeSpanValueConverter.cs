using System;

namespace Pancake.BakingSheet.Internal
{
    public class TimeSpanValueConverter : SheetValueConverter<TimeSpan>
    {
        protected override TimeSpan StringToValue(Type type, string value, SheetValueConvertingContext context) { return TimeSpan.Parse(value, context.FormatProvider); }

        protected override string ValueToString(Type type, TimeSpan value, SheetValueConvertingContext context) { return value.ToString(null, context.FormatProvider); }
    }
}