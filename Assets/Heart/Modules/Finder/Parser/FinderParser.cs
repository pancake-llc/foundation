using System;
using System.IO;
using UnityEditor;

namespace PancakeEditor.Finder
{
    internal static class FinderParser
    {
        public static void ReadYaml(string filePath, Action<string, long> callback) { Read(filePath, ParseLine_Yaml, callback); }

        public static void ReadJson(string filePath, Action<string, long> callback) { Read(filePath, ParseLine_Json, callback); }

        public static void ReadUssUxml(string filePath, Action<string, long> callback) { Read(filePath, ParseLine_Uxml_Uss, callback); }

        public static void ReadTss(string filePath, Action<string, long> callback) { Read(filePath, ParseLine_Tss, callback); }

        private static void Read(string filePath, Func<string, (string, long)> lineHandler, Action<string, long> add)
        {
            try
            {
                using (var sr = new StreamReader(filePath))
                {
                    while (sr.Peek() >= 0)
                    {
                        string line = sr.ReadLine();
                        if (string.IsNullOrEmpty(line)) continue;
                        int idx1 = line.IndexOf("guid", StringComparison.Ordinal);
                        int idx2 = line.IndexOf("m_AssetGUID", StringComparison.Ordinal);
                        if (idx1 == -1 && idx2 == -1) continue;

                        // compact line so that guid: xxxxx, fileID: xxxxx became guid:xxxxx,fileID:xxxxx
                        line = line.Replace(" ", string.Empty);
                        (string guid, long fileId) = lineHandler(line);
                        if (guid == null)
                        {
                            if (filePath.Contains("ProjectSettings/EditorBuildSettings.asset")) continue;
                        }

                        add(guid, fileId);
                    }
                }
            }
#pragma warning disable CS0168
            catch (Exception e)
            {
                // ignored
            }
#pragma warning restore CS0168
        }

        private static (string guid, long fileId) ParseLine_Yaml(string line)
        {
            // Check for both guid: and m_AssetGUID: patterns
            var result = FindRef(line, "guid:", "fileID:", ",");
            if (result.guid != null) return result;
            return FindRef(line, "m_AssetGUID:", null, null);
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

        private static string Find(string source, string strBegin, string strEnd)
        {
            int st = source.IndexOf(strBegin, StringComparison.Ordinal);
            if (st == -1) return null;
            st += strBegin.Length;
            
            if (string.IsNullOrEmpty(strEnd)) return source.Substring(st);

            int ed = source.IndexOf(strEnd, st, StringComparison.Ordinal);
            return ed == -1 ? null : source.Substring(st, ed - st);
        }
    }
}