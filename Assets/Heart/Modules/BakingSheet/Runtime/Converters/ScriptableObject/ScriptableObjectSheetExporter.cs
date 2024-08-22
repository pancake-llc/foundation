#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Pancake.BakingSheet.Internal;
using UnityEditor;
using UnityEngine;

namespace Pancake.BakingSheet.Unity
{
    public class ScriptableObjectSheetExporter : ISheetExporter, ISheetFormatter
    {
        private readonly string _savePath;

        public TimeZoneInfo TimeZoneInfo => TimeZoneInfo.Utc;
        public IFormatProvider FormatProvider => CultureInfo.InvariantCulture;

        public SheetContainerScriptableObject Result { get; private set; }

        public ScriptableObjectSheetExporter(string path) { _savePath = path; }

        public Task<bool> Export(SheetConvertingContext context)
        {
            var rowToSo = new Dictionary<ISheetRow, SheetRowScriptableObject>();

            var containerSo = GenerateAssets(_savePath, context, this, rowToSo);

            MapReferences(context, rowToSo);

            SaveAssets(containerSo);

            Result = containerSo;

            return Task.FromResult(true);
        }

        private static SheetContainerScriptableObject GenerateAssets(
            string savePath,
            SheetConvertingContext context,
            ISheetFormatter formatter,
            Dictionary<ISheetRow, SheetRowScriptableObject> rowToSO)
        {
            if (!AssetDatabase.IsValidFolder(savePath))
            {
                savePath = AssetDatabase.CreateFolder(Path.GetDirectoryName(savePath), Path.GetFileName(savePath));
            }

            var valueContext = new SheetValueConvertingContext(formatter, new SheetContractResolver());

            string containerPath = Path.Combine(savePath, "_Container.asset");

            var containerSo = AssetDatabase.LoadAssetAtPath<SheetContainerScriptableObject>(containerPath);

            if (containerSo == null)
            {
                containerSo = ScriptableObject.CreateInstance<SheetContainerScriptableObject>();
                AssetDatabase.CreateAsset(containerSo, containerPath);
            }

            containerSo.Clear();

            var existingRowSo = new Dictionary<string, SheetRowScriptableObject>();

            foreach (var pair in context.Container.GetSheetProperties())
            {
                var sheet = pair.Value.GetValue(context.Container) as ISheet;

                if (sheet == null)
                    continue;

                string sheetPath = Path.Combine(savePath, $"{sheet.Name}.asset");

                var sheetSo = AssetDatabase.LoadAssetAtPath<SheetScriptableObject>(sheetPath);

                if (sheetSo == null)
                {
                    sheetSo = ScriptableObject.CreateInstance<SheetScriptableObject>();
                    AssetDatabase.CreateAsset(sheetSo, sheetPath);
                }

                sheetSo.name = sheet.Name;
                sheetSo.typeInfo = sheet.RowType.FullName;

                existingRowSo.Clear();

                foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(sheetPath))
                {
                    if (!(asset is SheetRowScriptableObject rowSo)) continue;

                    string rowIdStr = rowSo.name;

                    existingRowSo.Add(rowIdStr, rowSo);
                }

                sheetSo.Clear();

                foreach (var row in sheet)
                {
                    string rowIdStr = valueContext.ValueToString(row.Id.GetType(), row.Id);

                    if (!existingRowSo.TryGetValue(rowIdStr, out var rowSo))
                    {
                        rowSo = ScriptableObject.CreateInstance<JsonSheetRowScriptableObject>();
                        AssetDatabase.AddObjectToAsset(rowSo, sheetSo);
                    }

                    rowSo.name = rowIdStr;
                    rowSo.SetRow(row);

                    sheetSo.Add(rowSo);
                    rowToSO.Add(row, rowSo);
                    existingRowSo.Remove(rowIdStr);
                }

                // clear removed scriptable objects
                foreach (var rowSo in existingRowSo.Values)
                    AssetDatabase.RemoveObjectFromAsset(rowSo);

                containerSo.Add(sheetSo);
            }

            return containerSo;
        }

        private static void MapReferences(SheetConvertingContext context, Dictionary<ISheetRow, SheetRowScriptableObject> rowToSo)
        {
            foreach (var pair in context.Container.GetSheetProperties())
            {
                var sheet = pair.Value.GetValue(context.Container) as ISheet;

                if (sheet == null)
                    continue;

                var propertyMap = sheet.GetPropertyMap(context);

                propertyMap.UpdateIndex(sheet);

                foreach (var (node, indexes) in propertyMap.TraverseLeaf())
                {
                    if (typeof(IUnitySheetReference).IsAssignableFrom(node.ValueType))
                    {
                        MapReferencesInSheet(context,
                            sheet,
                            node,
                            indexes,
                            SheetReferenceMapping,
                            rowToSo);
                    }
                    else if (typeof(IUnitySheetDirectAssetPath).IsAssignableFrom(node.ValueType))
                    {
                        MapReferencesInSheet(context,
                            sheet,
                            node,
                            indexes,
                            AssetReferenceMapping,
                            0);
                    }
                }
            }
        }

        private static void MapReferencesInSheet<TState>(
            SheetConvertingContext context,
            ISheet sheet,
            PropertyNode node,
            IEnumerable<object> indexes,
            Action<SheetConvertingContext, object, TState> mapper,
            TState state)
        {
            foreach (var row in sheet)
            {
                int verticalCount = node.GetVerticalCount(row, indexes.GetEnumerator());

                for (int vindex = 0; vindex < verticalCount; ++vindex)
                {
                    if (!node.TryGetValue(row, vindex, indexes.GetEnumerator(), out var obj))
                        continue;

                    mapper(context, obj, state);

                    // setting back for value type struct
                    if (node.ValueType.IsValueType)
                        node.SetValue(row, vindex, indexes.GetEnumerator(), obj);
                }
            }
        }

        private static void SheetReferenceMapping(SheetConvertingContext context, object obj, Dictionary<ISheetRow, SheetRowScriptableObject> rowToSO)
        {
            if (!(obj is IUnitySheetReference refer))
                return;

            if (!refer.IsValid())
                return;

            if (!rowToSO.TryGetValue(refer.Ref, out var asset))
            {
                Debug.LogError($"Failed to find reference \"{refer.Id}\" from Asset");
                return;
            }

            refer.Asset = asset;
        }

        private static void AssetReferenceMapping(SheetConvertingContext context, object obj, int _)
        {
            if (!(obj is IUnitySheetDirectAssetPath path))
                return;

            if (!path.IsValid())
                return;

            var fullPath = path.FullPath;

            UnityEngine.Object asset;

            if (string.IsNullOrEmpty(path.SubAssetName))
            {
                asset = AssetDatabase.LoadMainAssetAtPath(fullPath);
            }
            else
            {
                var assets = AssetDatabase.LoadAllAssetRepresentationsAtPath(fullPath);
                asset = assets.FirstOrDefault(x => x.name == path.SubAssetName);
            }

            if (asset == null)
            {
                UnityEngine.Debug.LogError($"Failed to find asset \"{fullPath}\" from Asset");
                return;
            }

            path.Asset = asset;
        }

        private static void SaveAssets(SheetContainerScriptableObject containerSo)
        {
            EditorUtility.SetDirty(containerSo);

            foreach (var sheetSo in containerSo.Sheets)
            {
                EditorUtility.SetDirty(sheetSo);

                foreach (var rowSo in sheetSo.Rows)
                    EditorUtility.SetDirty(rowSo);
            }

            AssetDatabase.SaveAssets();
        }
    }
}

#endif