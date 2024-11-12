using System;
using System.IO;
using UnityEditor;

namespace PancakeEditor.Finder
{
    internal static class FinderParser
    {
        public static void ReadYaml(string filePath, Action<string, long> callback) { Read(filePath, ParseLine_Yaml, callback); }

        public static void ReadJson(string filePath, Action<string, long> callback) { Read(filePath, ParseLine_Json, callback, false); }

        public static void ReadUssUxml(string filePath, Action<string, long> callback) { Read(filePath, ParseLine_Uxml_Uss, callback); }

        public static void ReadTss(string filePath, Action<string, long> callback) { Read(filePath, ParseLine_Tss, callback); }

        private static void Read(string filePath, Func<string, (string, long)> lineHandler, Action<string, long> add, bool doubleCheck = true)
        {
            try
            {
                using (var sr = new StreamReader(filePath))
                {
                    while (sr.Peek() >= 0)
                    {
                        string line = sr.ReadLine();
                        if (string.IsNullOrEmpty(line)) continue;

                        (string guid, long fileId) = lineHandler(line);
                        if (!string.IsNullOrEmpty(guid))
                        {
                            add(guid, fileId);
                            continue;
                        }

                        if (!doubleCheck) continue;
                        guid = ExtractGuid(line);
                        if (!string.IsNullOrEmpty(guid)) add(guid, 0);
                    }
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private static string ExtractGuid(string line)
        {
            const int GuidLength = 32;
            var validCharCount = 0;

            for (var i = 0; i < line.Length; i++)
            {
                // Check if the character is a valid hex character (0-9, a-f, A-F)
                if (IsHexChar(line[i]))
                {
                    validCharCount++;

                    if (validCharCount == GuidLength)
                    {
                        // Return substring from the start of the valid sequence
                        return line.Substring(i - GuidLength + 1, GuidLength);
                    }
                }
                else
                {
                    // Reset count if a non-hex character interrupts the sequence
                    validCharCount = 0;
                }
            }

            return null; // No valid GUID found
        }

        // Helper method to check if a character is a hex character
        private static bool IsHexChar(char c) => c is >= '0' and <= '9' or >= 'a' and <= 'f' or >= 'A' and <= 'F';

        private static (string guid, long fileId) ParseLine_Yaml(string line)
        {
            (string guid, long fileId) result = FindRef(line, "guid:", "fileID:", ",");
            if (result.guid != null) return result;
            
            result = FindRef(line, "m_AssetGUID:", null, null);
            if (!string.IsNullOrEmpty(result.guid)) return result;
            
            return FindRef(line, "GUID:", null, null);
        }

        private static (string guid, long fileId) ParseLine_Json(string line)
        {
            string guid = Find(line, "\\\"guid\\\":\\\"", "\\\",");
            if (string.IsNullOrEmpty(guid)) return (null, -1);

            string fileIdStr = Find(line, "\"fileID\\\":", ",");
            if (string.IsNullOrEmpty(fileIdStr)) return (null, -1);

            if (!long.TryParse(fileIdStr, out long fileId)) fileId = -1;
            return (guid, fileId);
        }

        private static (string guid, long fileId) ParseLine_Tss(string line)
        {
            string assetPath = Find(line, "@importurl(\"/", "\")");
            return string.IsNullOrEmpty(assetPath) ? (null, -1) : (AssetDatabase.AssetPathToGUID(assetPath), 0);
        }

        private static (string guid, long fileId) ParseLine_Uxml_Uss(string line) { return FindRef(line, "guid=", "fileID=", "&"); }

        private static (string guid, long fileId) FindRef(string source, string guidPattern, string fileIdPattern, string separatorPattern)
        {
            string guid = Find(source, guidPattern, separatorPattern);
            if (string.IsNullOrEmpty(guid)) return (null, -1);

            if (string.IsNullOrEmpty(fileIdPattern)) return (guid, -1);
            string fileIdStr = Find(source, fileIdPattern, separatorPattern);
            if (string.IsNullOrEmpty(fileIdStr)) return (null, -1);

            if (!long.TryParse(fileIdStr, out long fileId)) fileId = -1;
            return (guid, fileId);
        }

        private static string Find(string source, string str_begin, string str_end)
        {
            int st = source.IndexOf(str_begin, StringComparison.Ordinal);
            if (st == -1) return null;
            st += str_begin.Length;
            while (char.IsWhiteSpace(source[st]) && st < source.Length-1) st++;
            if (string.IsNullOrEmpty(str_end)) // no end: determine by length
            {
                return source.Substring(st, 32);
            }
            
            int ed = source.IndexOf(str_end, st, StringComparison.Ordinal);
            if (ed == -1) return null;
            while (char.IsWhiteSpace(source[ed])) ed--;
            return source.Substring(st, ed - st);
        }
    }
}