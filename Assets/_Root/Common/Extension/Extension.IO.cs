using System;
using System.IO;
using UnityEngine;

namespace Pancake.Core
{
    public static partial class C
    {
        /// <summary>
        /// Get time for last change file with <paramref name="filePath"/>
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static DateTime GetTimestamp(this string filePath)
        {
            if (!File.Exists(filePath))
                return new DateTime(1970,
                    1,
                    1,
                    0,
                    0,
                    0,
                    0,
                    System.DateTimeKind.Utc);
            return File.GetLastWriteTime(filePath).ToUniversalTime();
        }

        /// <summary>
        /// Delete file if it exist with path <paramref name="filePath"/>
        /// </summary>
        /// <param name="filePath"></param>
        public static void DeleteFile(this string filePath)
        {
            if (File.Exists(filePath)) File.Delete(filePath);
        }

        /// <summary>
        /// Get extension of file
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetExtension(this string path) { return Path.GetExtension(path); }

        /// <summary>
        /// Indicate if the file with path <paramref name="filePath"/> exists?
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static bool FileExists(this string filePath) { return File.Exists(filePath); }

        /// <summary>
        /// Move file from <paramref name="sourcePath"/> to <paramref name="destPath"/>
        /// </summary>
        /// <param name="sourcePath"></param>
        /// <param name="destPath"></param>
        public static void MoveFile(this string sourcePath, string destPath) { File.Move(sourcePath, destPath); }

        /// <summary>
        /// Copy file from <paramref name="sourcePath"/> to <paramref name="destPath"/>
        /// </summary>
        /// <param name="sourcePath"></param>
        /// <param name="destPath"></param>
        public static void CopyFile(this string sourcePath, string destPath) { File.Copy(sourcePath, destPath); }

        /// <summary>
        /// Move dictionary from <paramref name="sourcePath"/> to <paramref name="destPath"/>
        /// </summary>
        /// <param name="sourcePath"></param>
        /// <param name="destPath"></param>
        public static void MoveDirectory(this string sourcePath, string destPath) { Directory.Move(sourcePath, destPath); }

        /// <summary>
        /// Create dictionary with <paramref name="directoryPath"/>
        /// </summary>
        /// <param name="directoryPath"></param>
        public static void CreateDirectory(this string directoryPath) { Directory.CreateDirectory(directoryPath); }

        /// <summary>
        /// Indicate if the directory with path <paramref name="directoryPath"/> exists?
        /// </summary>
        /// <param name="directoryPath"></param>
        /// <returns></returns>
        public static bool DirectoryExists(this string directoryPath) { return Directory.Exists(directoryPath); }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="seperator"></param>
        /// <returns></returns>
        public static string GetDirectoryPath(this string path, char seperator = '/')
        {
            // Substring is used instead.
            char slashChar = UsesForwardSlash(path) ? '/' : '\\';

            int slash = path.LastIndexOf(slashChar);

            // Ignore trailing slash if necessary.
            if (slash == (path.Length - 1)) slash = path.Substring(0, slash).LastIndexOf(slashChar);
            if (slash == -1) Debug.LogError("Path provided is not a directory path as it contains no slashes.");
            return path.Substring(0, slash);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool UsesForwardSlash(string path)
        {
            if (path.Contains("/")) return true;
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="directoryPath"></param>
        /// <param name="fileOrDirectoryName"></param>
        /// <returns></returns>
        public static string CombinePathAndFilename(this string directoryPath, string fileOrDirectoryName)
        {
            if (directoryPath[directoryPath.Length - 1] != '/' && directoryPath[directoryPath.Length - 1] != '\\') directoryPath += '/';
            return directoryPath + fileOrDirectoryName;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="getFullPaths"></param>
        /// <returns></returns>
        public static string[] GetDirectories(this string path, bool getFullPaths = true)
        {
            var paths = Directory.GetDirectories(path);
            for (int i = 0; i < paths.Length; i++)
            {
                if (!getFullPaths) paths[i] = Path.GetFileName(paths[i]);
                // GetDirectories sometimes returns backslashes, so we need to convert them to
                // forward slashes.
                paths[i] = paths[i].Replace("\\", "/");
            }

            return paths;
        }

        /// <summary>
        /// Delete directory if it exist with path <paramref name="directoryPath"/>
        /// </summary>
        /// <param name="directoryPath"></param>
        public static void DeleteDirectory(this string directoryPath)
        {
            if (Directory.Exists(directoryPath)) Directory.Delete(directoryPath, true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="getFullPaths"></param>
        /// <returns></returns>
        public static string[] GetFiles(this string path, bool getFullPaths = true)
        {
            var paths = Directory.GetFiles(path);
            if (!getFullPaths)
            {
                for (int i = 0; i < paths.Length; i++) paths[i] = Path.GetFileName(paths[i]);
            }

            return paths;
        }
        
    }
}