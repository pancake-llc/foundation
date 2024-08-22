using System;
using System.Reflection;

namespace Pancake.BakingSheet
{
    /// <summary>
    /// Interface for contract resolver that determines and caches value converter.
    /// </summary>
    public interface ISheetContractResolver
    {
        ISheetValueConverter GetValueConverter(Type type);
        ISheetValueConverter GetValueConverter(PropertyInfo propertyInfo);
    }
}