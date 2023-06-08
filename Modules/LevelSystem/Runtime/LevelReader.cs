using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace Pancake.LevelSystem
{
    public static class LevelReader
    {
        private const string FOLDER_NAME = "LevelSystem";

        /// <summary>
        /// Read the data of a specific level from the json files under the Resources/LevelSystem folder
        /// </summary>
        /// <param name="setting"></param>
        /// <param name="levelNumber"></param>
        /// <returns></returns>
        public static LevelNode Read(LevelSystemSetting setting, int levelNumber)
        {
            if (levelNumber < 1) return null;

            int fileNumber = (levelNumber - 1) / setting.LevelPerFile + 1;
            int levelIndex = levelNumber % setting.LevelPerFile;
            levelIndex = levelIndex == 0 ? setting.LevelPerFile - 1 : levelIndex - 1;

            string path = Path.Combine(FOLDER_NAME, string.Concat(setting.FileName, "_", fileNumber.ToString()));
            var json = Resources.Load<TextAsset>(path);
            if (json == null) return null;

            var levelInfo = JsonConvert.DeserializeObject<LevelData>(json.text);
            if (levelInfo is {levelNodes: not null} && levelInfo.levelNodes.Length > levelIndex) return levelInfo.levelNodes[levelIndex];

            return null;
        }

        /// <summary>
        /// Count the number of all the existing levels in all the json files
        /// </summary>
        /// <param name="setting"></param>
        /// <returns></returns>
        public static int GetLevelCount(LevelSystemSetting setting)
        {
            var count = 0;
            for (var i = 0; i < 10000; i++)
            {
                string path = Path.Combine(FOLDER_NAME, string.Concat(setting.FileName, "_", i.ToString()));
                var json = Resources.Load<TextAsset>(path);
                if (json == null) return count;

                var levelInfo = JsonConvert.DeserializeObject<LevelData>(json.text);
                if (levelInfo?.levelNodes == null) return count;

                count += levelInfo.levelNodes.Length;
            }

            return count;
        }

        /// <summary>
        /// Write the level data at the specified position
        /// </summary>
        /// <param name="setting"></param>
        /// <param name="level"></param>
        /// <param name="levelNumber"></param>
        public static string Write(LevelSystemSetting setting, LevelNode level, int levelNumber)
        {
            if (levelNumber < 1) return "Level must be greater than zero";

            int levelCount = GetLevelCount(setting);
            if (levelNumber > levelCount + 1) // you can save levels in a progressive way ( you can't leave empty spots )
            {
                return $"The next level you can save is {levelCount + 1}";
            }

            int fileNumber = (levelNumber - 1) / setting.LevelPerFile + 1;
            int levelIndex = levelNumber % setting.LevelPerFile;
            levelIndex = levelIndex == 0 ? setting.LevelPerFile - 1 : levelIndex - 1;
            string path = Path.Combine(FOLDER_NAME, string.Concat(setting.FileName, "_", fileNumber.ToString()));
            var json = Resources.Load<TextAsset>(path);

            LevelData data;
            if (json == null)
            {
                data = new LevelData {levelNodes = Array.Empty<LevelNode>()};
            }
            else
            {
                data = JsonConvert.DeserializeObject<LevelData>(json.text);
            }

            var newLevels = new List<LevelNode>();
            for (var i = 0; i < data.levelNodes.Length; i++)
            {
                newLevels.Add(data.levelNodes[i]);
            }

            if (level == null) // if null data, remove this level from the list
            {
                newLevels.RemoveAt(levelIndex);
            }
            else if (data.levelNodes.Length <= levelIndex) // if the current array is full, add a space for the new level
            {
                newLevels.Add(level);
            }
            else
            {
                newLevels[levelIndex] = level; // overwrite in other cases
            }

            data.levelNodes = newLevels.ToArray();
            string jsonData = JsonConvert.SerializeObject(data);

            string resultPath = Path.Combine("Assets", "Resources", FOLDER_NAME, setting.FileName + "_" + fileNumber + ".json");
            string folderPath = Path.Combine("Assets", "Resources", FOLDER_NAME);
            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

            StreamWriter writer = null;
            try
            {
                writer = new StreamWriter(resultPath, false);
                writer.WriteLine(jsonData);
                writer.Close();
            }
            catch (IOException e)
            {
                writer?.Close();
                return "Error during file saving: " + e.Message;
            }

#if UNITY_EDITOR
            AssetDatabase.ImportAsset(resultPath);
#endif
            return "";
        }

        /// <summary>
        /// Delete the specified level from the files and reorganize the next levels to cover the empty space
        /// </summary>
        /// <param name="setting"></param>
        /// <param name="levelNumber"></param>
        /// <returns></returns>
        public static LevelNode DeleteLevel(LevelSystemSetting setting, int levelNumber)
        {
            if (levelNumber < 1) return null;

            int totalCount = GetLevelCount(setting);
            if (levelNumber > totalCount) return null;

            // overwrite each level to the previous position, starting from the level to delete
            for (int i = levelNumber; i < totalCount; i++)
            {
                var nextLevel = Read(setting, i + 1);
                if (nextLevel == null) break; // this case should not happen
                nextLevel.level = i;
                Write(setting, nextLevel, i);
            }

            Write(setting, null, totalCount); // after reassigning all the levels, delete the last one

            // check the last json file, if it's empty, delete it from the Resources folder
            int fileNumber = (totalCount - 1) / setting.LevelPerFile + 1;
            var fileNumberString = fileNumber.ToString();

            string filePath = Path.Combine(FOLDER_NAME, string.Concat(setting.FileName, "_", fileNumberString));
            var jsonTextFile = (TextAsset) Resources.Load(filePath, typeof(TextAsset));

            if (jsonTextFile == null || JsonConvert.DeserializeObject<LevelData>(jsonTextFile.text).levelNodes.Length == 0)
            {
                // delete it
                string path = Path.Combine("Assets", "Resources", FOLDER_NAME, setting.FileName + "_" + fileNumberString + ".json");
                try
                {
                    File.Delete(path);
                }
                catch (IOException)
                {
                    return null;
                }

                //Re-import the file to update the reference in the editor
#if UNITY_EDITOR
                AssetDatabase.ImportAsset(path);
#endif
            }

            return null;
        }
    }
}