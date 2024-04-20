using System;

namespace Pancake.BakingSheet.Internal
{
    public class AssetPathValueConverter : SheetValueConverter<ISheetAssetPath>
    {
        protected override ISheetAssetPath StringToValue(Type type, string value, SheetValueConvertingContext context)
        {
            return (ISheetAssetPath) Activator.CreateInstance(type, value);
        }

        protected override string ValueToString(Type type, ISheetAssetPath value, SheetValueConvertingContext context)
        {
            if (!value.IsValid())
                return null;

            return value.RawValue;
        }
    }
}