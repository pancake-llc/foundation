using System;
using System.Threading.Tasks;


namespace Pancake.BakingSheet.Unity
{
    public class ScriptableObjectSheetImporter : ISheetImporter
    {
        private readonly SheetContainerScriptableObject _so;

        public ScriptableObjectSheetImporter(SheetContainerScriptableObject so) { _so = so; }

        public Task<bool> Import(SheetConvertingContext context)
        {
            var sheetProperties = context.Container.GetSheetProperties();

            foreach (var sheetSo in _so.Sheets)
            {
                string sheetName = sheetSo.name;

                if (!sheetProperties.TryGetValue(sheetName, out var prop))
                {
                    UnityEngine.Debug.LogError($"Failed to find sheet: {sheetName}");
                    continue;
                }

                var sheet = (ISheet) Activator.CreateInstance(prop.PropertyType);

                foreach (var rowSo in sheetSo.Rows)
                {
                    var row = rowSo.GetRow(sheet.RowType);
                    sheet.Add(row);
                }

                prop.SetValue(context.Container, sheet);
            }

            // _so.Sheets
            return Task.FromResult(true);
        }
    }
}