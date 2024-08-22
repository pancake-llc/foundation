using System.Collections.Generic;
using System.IO;
using System.Linq;
using Pancake.Csv;
using Pancake.Localization;
using UnityEditor;
using UnityEngine;

namespace PancakeEditor.Localization
{
    internal class CsvSerialization
    {
        private const string KEY_COLUMN = "Key";

        private class CsvTable : List<List<string>>
        {
            public List<string> AddRow()
            {
                var row = new List<string>();
                Add(row);
                return row;
            }
        }

        public void Serialize(TextWriter writer)
        {
            var languages = LocaleSettings.AvailableLanguages;
            var localizedTexts = Locale.FindAllLocalizedAssets<LocaleText>();

            var csvWriter = new CsvWriter(writer);
            csvWriter.WriteField(KEY_COLUMN);
            foreach (var lang in languages)
            {
                csvWriter.WriteField(lang.Code);
            }

            csvWriter.NextRecord();

            foreach (var localizedText in localizedTexts)
            {
                csvWriter.WriteField(localizedText.name);

                foreach (var lang in languages)
                {
                    if (!localizedText.TryGetLocaleValue(lang, out string value)) value = "";

                    csvWriter.WriteField(value);
                }

                csvWriter.NextRecord();
            }
        }

        public void Deserialize(TextReader reader)
        {
            string importLocation = LocaleSettings.ImportLocation;
            var localizedTexts = Locale.FindAllLocalizedAssets<LocaleText>();

            var csv = new CsvReader(reader);
            var table = new CsvTable();
            var languages = new List<Language>();

            while (csv.Read())
            {
                var row = table.AddRow();
                for (var i = 0; i < csv.FieldsCount; ++i) row.Add(csv[i]);
            }

            var availableLanguages = LocaleSettings.AllLanguages;
            for (var i = 1; i < table[0].Count; i++)
            {
                var language = availableLanguages.FirstOrDefault(x => x.Code == table[0][i]);
                if (language == null) Debug.LogWarning("Language code (" + table[0][i] + ") not exist in localization system.");

                // Add null language as well to maintain order.
                languages.Add(language);
            }

            for (var i = 1; i < table.Count; i++)
            {
                var row = table[i];
                string key = row[0];
                var localizedText = localizedTexts.FirstOrDefault(x => x.name == key);
                if (localizedText == null)
                {
                    localizedText = ScriptableObject.CreateInstance<LocaleText>();

                    string assetPath = Path.Combine(importLocation, $"{key}.asset");
                    AssetDatabase.CreateAsset(localizedText, assetPath);
                    AssetDatabase.SaveAssets();
                }

                // Read languages by ignoring first column (Key).
                for (var j = 1; j < row.Count; j++)
                {
                    ScriptableLocaleEditor.AddOrUpdateLocale(localizedText, languages[j - 1], row[j]);
                }

                EditorUtility.SetDirty(localizedText);
            }

            AssetDatabase.Refresh();
        }
    }
}