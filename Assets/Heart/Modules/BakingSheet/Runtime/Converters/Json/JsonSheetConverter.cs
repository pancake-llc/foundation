using System;
using System.IO;
using System.Threading.Tasks;
using Pancake.BakingSheet.Internal;
using Newtonsoft.Json;
using ErrorEventArgs = Newtonsoft.Json.Serialization.ErrorEventArgs;

namespace Pancake.BakingSheet
{
    public class JsonSheetConverter : ISheetConverter
    {
        public virtual string Extension => "json";

        private string _loadPath;
        private IFileSystem _fileSystem;

        public JsonSheetConverter(string path, IFileSystem fileSystem = null)
        {
            _loadPath = path;
            _fileSystem = fileSystem ?? new FileSystem();
        }

        public static void ErrorHandler(ErrorEventArgs err)
        {
            if (err.ErrorContext.Member?.ToString() == nameof(ISheetRow.Id) && err.ErrorContext.OriginalObject is ISheetRow && !(err.CurrentObject is ISheet))
            {
                // if Id has error, the error must be handled on the sheet level
                return;
            }

            UnityEngine.Debug.LogError(err.ErrorContext.Error.Message);

            err.ErrorContext.Handled = true;
        }

        public virtual JsonSerializerSettings GetSettings()
        {
            var settings = new JsonSerializerSettings {Error = (_, err) => ErrorHandler(err), ContractResolver = JsonSheetContractResolver.Instance};
            return settings;
        }

        protected virtual string Serialize(object obj, Type type) { return JsonConvert.SerializeObject(obj, type, GetSettings()); }

        protected virtual object Deserialize(string json, Type type) { return JsonConvert.DeserializeObject(json, type, GetSettings()); }

        public async Task<bool> Import(SheetConvertingContext context)
        {
            var sheetProps = context.Container.GetSheetProperties();

            foreach (var pair in sheetProps)
            {
                string path = Path.Combine(_loadPath, $"{pair.Key}.{Extension}");

                if (!_fileSystem.Exists(path))
                    continue;

                string data;

                using (var stream = _fileSystem.OpenRead(path))
                using (var reader = new StreamReader(stream))
                    data = await reader.ReadToEndAsync();

                var sheet = Deserialize(data, pair.Value.PropertyType) as ISheet;
                pair.Value.SetValue(context.Container, sheet);
            }

            return true;
        }

        public async Task<bool> Export(SheetConvertingContext context)
        {
            var sheetProps = context.Container.GetSheetProperties();

            _fileSystem.CreateDirectory(_loadPath);

            foreach (var pair in sheetProps)
            {
                var sheet = pair.Value.GetValue(context.Container);
                var data = Serialize(sheet, pair.Value.PropertyType);

                var path = Path.Combine(_loadPath, $"{pair.Key}.{Extension}");

                using (var stream = _fileSystem.OpenWrite(path))
                using (var writer = new StreamWriter(stream))
                    await writer.WriteAsync(data);
            }

            return true;
        }
    }
}